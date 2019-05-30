from challenges import puzzle
import requests
from multiprocessing import Process, Queue
import time

def make_request():
    print('requesting a challenge')
    r = requests.get('http://127.0.0.1:5000/ChallengeMe', json={"key": "value"})
    json = r.json()
    header = json['header'].encode('utf-16')[2:]
    target = json['target']
    #puzzle_id = json['id']
    print('received a puzzle')
    new_header = puzzle.solve_puzzle(header, target).decode('utf-16')
    print('solved the puzzle, sending to server')
    r = requests.get('http://127.0.0.1:5000/', 
        json = {
            # "id" : puzzle_id # echo the id back to the server
            "header" : new_header,
            "target" : target
        }
    )
    print('server responded to solution with: ' + str(r.json()))

    puzzle_success = 1 if r.json()['access'] == True else 0
    block_success = 1 if r.json()['block_conf'] == True else 0
    return puzzle_success, block_success


def client_worker(result_queue, n_req):
    puzzle_success_count = 0
    block_success_count = 0
    for i in range(n_req):
        p_success, b_success = make_request()
        puzzle_success_count += p_success
        block_success_count += b_success

    result_queue.put((puzzle_success_count, block_success_count))
    
def sum_queue(n_processes, que):
    a = 0
    b = 0
    for i in range(n_processes):
        tup = que.get()
        a += tup[0]
        b += tup[1]
    return a, b

if __name__ == '__main__':
    print('hello')
    n_req = 100
    result_queue = Queue()

    start_time = time.time()

    job_list = []
    for i in range(16):
        new_proc = Process(target=client_worker, args=(result_queue, n_req))
        new_proc.daemon = True
        new_proc.start()
        job_list.append(new_proc)

    for proc in job_list:
        proc.join()

    end_time = time.time()

    puzzle_success_count, block_success_count = sum_queue(16, result_queue)

    print('n requests: %d' % n_req)
    print('n puzzles solved: %d' % puzzle_success_count)
    print('n blocks solved: %d' % block_success_count)
    print('time taken (s): %s' % (end_time - start_time))
    