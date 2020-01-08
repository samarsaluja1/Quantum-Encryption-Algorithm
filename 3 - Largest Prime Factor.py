import math
NUM=6
root = math.ceil(math.sqrt(NUM))

def isPrime(x):
    if x == 2:
        return True
    
    for i in range(2, math.ceil(math.sqrt(x)) + 1):
        if x % i == 0:
            return False

    return True

for i in range(root, 1, -1):
    if NUM % i == 0 and isPrime(i):
        print(i, NUM // i, isPrime(NUM//i))
    if NUM % i == 0 and isPrime(NUM//i):
        print(i, NUM // i, isPrime(i))
        

