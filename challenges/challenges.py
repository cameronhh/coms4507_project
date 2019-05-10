import random
import hashlib

def challenge(n_bits, n_hashes):
    ''' Generates random inputs and hashes them until a hash < solution is
        found or n_hashes have been performed.
        m = SHA256(SHA256(c)) (bitcoin is a good place to start)
    '''
    hash_list = []
    for i in range(n_hashes):
        m1 = hashlib.sha256()
        m1.update(str(random.getrandbits(n_bits)).encode('utf-8')) # ugly, but twice as fast as formatting
        m2 = hashlib.sha256()
        m2.update(m1.digest())
        hash_list.append(m2.digest())
    return hash_list

print(challenge(32,100))
