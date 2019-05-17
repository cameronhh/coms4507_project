import random
import struct
import hashlib
import random
import codecs

# todo:
    # add comments
    # one small line to incorporate challenge difficulty
    # save a block header with a client session (flask)
    # time counter in flask for incrementing timestamp on blocks
    # clean up


def _convert_bytes_to_long(hash):
    return int.from_bytes(hash, byteorder='little', signed=False)

def get_block():
    ''' Returns the current block the server is working on, as bytes.
        Last 4 bytes are the nonce
    '''
    # mock block header, values for version_num, prev_header, root_hash, target,
    # timestamp are all constant (as far as the requesting in client is concerned)

    ### 18b
    version_num = 0xFF
    prev_header = 0x3e18ba72 # shortened for simplicity
    root_hash = 0x996d90de # as above

    block_target = b'\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\f\0\0' # not used by client, but needs to be given to the client to make work useful

    ### 8b
    timestamp = 1 # have current 'time' counter in server and just add 1 every time
    nonce = 0xFFFF

    block_header = struct.pack('<hqq', version_num, prev_header, root_hash) + block_target + struct.pack('ii', timestamp, nonce)
    return block_header

def generate_puzzle(difficulty):
    ''' Generates a puzzle for a given difficulty.
        Assumption that APIs have discrete request difficulty.
        (Or some assigned, discrete difficulty for continuous values.)
        Returns dict with block_header and client_target, because
        client_target != block_target (obvs)

        Returns block_header, 256-bit long
    '''
    #16-bit preimage for e.g.
    target = b'\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\f\0'
    target = _convert_bytes_to_long(target)

    return get_block(), target

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


if __name__=="__main__":
    header, target = generate_puzzle(8)
    #print(target)
    print("HEADER 1" + str(header))
    print("HEADER 2" + str(header.decode('utf-16').encode('utf-16')[2:]))

    encoded4 = codecs.decode(header, 'utf-16')
    print(encoded4)
    decoded = codecs.encode(encoded4, 'utf-16')
    print(decoded)

    client_header = solve_puzzle(header, target)
    result = validate_puzzle(client_header, target)
    print(result)
    print(_check_block_confirmation(client_header))
