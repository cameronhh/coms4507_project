import requests
from challenges import puzzle

if __name__=="__main__":
    print('requesting a challenge')
    r = requests.get('http://127.0.0.1:5000/ChallengeMe', json={"key": "value"})
    json = r.json()
    header = json['header'].encode('utf-16')[2:]
    target = json['target']
    #puzzle_id = json['id']
    print('Solving puzzle header ' + str(header) + ' with target ' + str(target))
    new_header = puzzle.solve_puzzle(header, target).decode('utf-16')
    print('puzzle solved, sending to server')
    r = requests.get('http://127.0.0.1:5000/', 
        json = {
            # "id" : puzzle_id # echo the id back to the server
            "header" : new_header,
            "target" : target
        }
    )
    print('server responded to solution with: ' + str(r.json()))
