namespace Microsoft.Quantum.Samples.IntegerFactorization {
    
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Extensions.Math;
    open Microsoft.Quantum.Extensions.Convert;
    open Microsoft.Quantum.Canon;

    /// Phase estimation by Microsoft.Quantum.Canon.RobustPhaseEstimation or Microsoft.Quantum.Canon.QuantumPhaseEstimation 
	//Takes in an integer as input, Outputting two prime factors
    operation Shor (number : Int, useRobustPhaseEstimation : Bool) : (Int, Int) {
        


        //Check divisibility by 2
        if (number % 2 == 0) {
            Message("An even number has been passed; 2 is the factor.");
            return (number / 2, 2);
        }

        // Generate and Check if integers are Co-Prime
        let coprimeCandidate = RandomInt(number - 1);
        if (IsCoprime(coprimeCandidate, number)) {
            
            Message($"Estimating period of {coprimeCandidate}");
            

            // Quantum Period Finding Algorithm
            let period = EstimatePeriod(coprimeCandidate, number, useRobustPhaseEstimation);
            
            // Check if Period is Even - Must be Even
            if (period % 2 == 0) {
                
                // Compute `coprimeCandidate` ^ `period/2` mod `number`
                let halfPower = ExpMod(coprimeCandidate, period / 2, number);
                
                // If result is '-1 mod N', the case not useful for factoring. Check if this is not the case.
                if (halfPower != number - 1) {
                    
                    // halfPower-1 or halfPower+1 shares a non-trivial divisor with `number`. Function that finds this divisor.
                    let factor = MaxI(GCD(halfPower - 1, number), GCD(halfPower + 1, number));
                    
                    // Return computed prime factors.
                    return (factor, number / factor);
                }
                else {
                    
                    // Algorithm fails due to trivial case.
                    fail "Residue xᵃ = -1 (mod N) where a is a period.";
                }
            }
            else {
                
                // Algorithm fails because period is odd.
                fail "Period is odd.";
            }
        }
        // Divisor by accident
        else {
            
            // Find a Divsor using GCD function
            let gcd = GCD(number, coprimeCandidate);
            Message($"We have guessed a divisor of {number} to be {gcd} by accident.");
            return (gcd, number / gcd);
        }
    }
    
    
    // Interprets `target` as encoding unsigned little-endian integer k and performs transformation |k⟩ ↦ |gᵖ⋅k mod N ⟩ -- Quibit Manipulation
    operation OrderFindingOracle (generator : Int, modulus : Int, power : Int, target : Qubit[]) : Unit {
        
        body (...) {
            // Check that the parameters satisfy the requirements.
            AssertBoolEqual(IsCoprime(generator, modulus), true, "`generator` and `modulus` must be co-prime");
            
            // Qubit is interpreted as unsigned integer in little-endian encoding - Microsoft.Quantum.Canon.LittleEndian type.
            ModularMultiplyByConstantLE(ExpMod(generator, power, modulus), modulus, LittleEndian(target));
        }
        
        adjoint invert;
        //OrderFindingOracle must have a controlled version of the oracle
        controlled distribute;
        controlled adjoint distribute;
    }
    
    
    /// # Summary
    /// Finds a multiplicative order of the generator in the residue ring Z mod `modulus`.
    operation EstimatePeriod (generator : Int, modulus : Int, useRobustPhaseEstimation : Bool) : Int {
        
        // Check that inputs are valid.
        AssertBoolEqual(IsCoprime(generator, modulus), true, "`generator` and `modulus` must be co-prime");
        
        // Stores the divisor of the generator period.
        mutable result = 1;
        
        // Number of bits in the modulus with respect to which we are estimating the period.
        let bitsize = BitSize(modulus);
        
        // The EstimatePeriod operation estimates the period r by finding an approximation k/2^bitsPrecision to a fraction s/r where s is some integer.
        let bitsPrecision = 2 * bitsize + 1;
        
        repeat {
            
            // The variable that stores numerator of dyadic fraction k/2^bitsPrecision, approximating s/r
            mutable dyadicFractionNum = 0;
            
            // Allocate qubits for the superposition of eigenstates of the oracle that is used in period finding.
            using (eignestateRegister = Qubit[bitsize]) {
                
                // Initialize eignestateRegister to 1 which is a superposition of the eigenstates we are estimating the phases of. 
				//Register is interpreted in little endian coding.
                let eignestateRegisterLE = LittleEndian(eignestateRegister);
                InPlaceXorLE(1, eignestateRegisterLE);
                
                let oracle = DiscreteOracle(OrderFindingOracle(generator, modulus, _, _));
                
                // Find the numerator of a dyadic fraction that approximates s/r where r is the multiplicative order ( period ) of g
                if (useRobustPhaseEstimation) {
                    
                    // Use Microsoft.Quantum.Canon.RobustPhaseEstimation to estimate s/r.
                    let phase = RobustPhaseEstimation(bitsPrecision, oracle, eignestateRegisterLE!);
                    
                    // Compute the numerator k of dyadic fraction k/2^bitsPrecision, approximating s/r.
                    set dyadicFractionNum = Round(((phase * ToDouble(2 ^ bitsPrecision)) / 2.0) / PI());
                }
                else {
                    
                    // Use Microsoft.Quantum.Canon.QuantumPhaseEstimation to estimate s/r.
                    using (dyadicFractionNumerator = Qubit[bitsPrecision]) {
                        
                        // The register that will contain the numerator k of dyadic fraction k/2^bitsPrecision. 
						// The numerator is unsigned integer encoded in big-endian format. 
                        let dyadicFractionNumeratorBE = BigEndian(dyadicFractionNumerator);
                        QuantumPhaseEstimation(oracle, eignestateRegisterLE!, dyadicFractionNumeratorBE);
                        
                        // Directly measure the numerator k of dyadic fraction k/2^bitsPrecision, approximating s/r. 
                        set dyadicFractionNum = MeasureIntegerBE(dyadicFractionNumeratorBE);
                    }
                }
                
                // Return all the qubits used for oracle's eigenstate back to 0 state
                ResetAll(eignestateRegister);
            }			
            
            // This will print our estimate of s/r to the standard output
            Message($"Estimated eigenvalue is {dyadicFractionNum}/2^{bitsPrecision}.");
            
            // Recover s/r from dyadic fraction k/2^bitsPrecision.
            let (numerator, period) = (ContinuedFractionConvergent(Fraction(dyadicFractionNum, 2 ^ bitsPrecision), modulus))!;
            
			// Make sure that numerator and denominator are positive 
            let (numeratorAbs, periodAbs) = (AbsI(numerator), AbsI(period));
            
            // Output the period divisor and the eigenstate number
            Message($"Estimated divisor of period is {periodAbs}, " + $" we have projected on eigenstate marked by {numeratorAbs}.");
            
            // Update the result variable by including newly found divisor.
            set result = (periodAbs * result) / GCD(result, periodAbs);
        }
        until (ExpMod(generator, result, modulus) == 1)
        fixup {
            
            // Above we checked if we have found actual period, or only the divisor of it. If the period was found, loop terminates.
            // If we have not found the period, output message about it to standard output and try again.
            Message("It looks like the period has divisors and we have " + "found only a divisor of the period. Trying again ...");
        }
        
        // Return found period.
        return result;
    }
    
}

