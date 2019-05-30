import random
import struct
import hashlib

# todo:
    # add comments
    # save a block header with a client session (flask)
    # time counter in flask for incrementing timestamp on blocks

def _convert_bytes_to_long(some_bytes):
    return int.from_bytes(some_bytes, byteorder='little', signed=False)

def get_block():
    ''' Returns the current BLOCK_HEADER the server is 'working on', as bytes.
        Last 4 bytes are the nonce
    '''
    # mock block header, values for version_num, prev_header, root_hash, target,
    # timestamp are all immutable by the client (else validation will fail)
    # timestamp unique for each client until a new block header is retrieved
    # from a 'full node'

    ### 18B
    version_num = 0xFF
    prev_header = 0x3e18ba72 # shortened for simplicity
    root_hash = 0x996d90de # as above
    block_target = 2**(240) - 1 # arbitrarily small
    block_target = block_target.to_bytes(block_target.bit_length(), byteorder='little', signed=False)

    ### 8B
    timestamp = random.randint(0, 16192)
    nonce = 0xFFFF

    block_header = struct.pack('<hqq', version_num, prev_header, root_hash) + block_target + struct.pack('ii', timestamp, nonce)
    return block_header

def generate_puzzle(difficulty):
    ''' input: difficulty is an integer 'n', such that the CLIENT_TARGET will
        require finding a n-bit pre-image to solve.

        return: a BLOCK_HEADER (type=bytes), as returned by get_block(), and
        the CLIENT_TARGET (type=256 bit int) (referred to as 'Tcp' in the report).
    '''
    client_target = 2**(256-difficulty) - 1
    return get_block(), client_target

def validate_puzzle(block_header, target):
    ''' Checks that the returned block header is valid, and
        that hash(block_header) < target.
    '''
    hasher_1 = hashlib.sha256()
    hasher_2 = hashlib.sha256()
    hasher_1.update(block_header)
    hasher_2.update(hasher_1.digest())
    hash = hasher_2.digest()
    l_hash = _convert_bytes_to_long(hash)
    return l_hash < target

def _check_block_confirmation(block_header):
    ''' Checks to see if the given block header (which has been returned
        by a client and validated as a puzzle solution) is also a valid block
        confirmation.
    '''
    # get target from block
    l_target = _convert_bytes_to_long(block_header[18:-8])
    return validate_puzzle(block_header, l_target)

def solve_puzzle(block_header, target_difficulty):
    # only last 4 bytes (nonce) are adjusted by client
    static_data = block_header[:-4]
    pre_hash = hashlib.sha256()
    pre_hash.update(static_data)
    solution = None
    for nonce in range(2**32):
        nonce = struct.pack("<i", nonce)
        # copy pre hash and add nonce to it
        hasher_1 = pre_hash.copy()
        hasher_1.update(nonce)
        hasher_2 = hashlib.sha256()
        hasher_2.update(hasher_1.digest())

        hash = hasher_2.digest()
        l_hash = _convert_bytes_to_long(hash)

        if l_hash < target_difficulty: #solved
            solution = nonce
            break
    return static_data + nonce


#usage:
header, target = generate_puzzle(21)
client_header = solve_puzzle(header, target)
result = validate_puzzle(client_header, target)
print(result)
print(_check_block_confirmation(client_header))
