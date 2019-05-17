from flask import Flask, jsonify, request
import requests
from challenges import puzzle

app = Flask(__name__)



@app.route('/ChallengeMe')
def getChallenge():
	# generate challenge
	header, target = puzzle.generate_puzzle(8)
	header_str=header.decode('utf-16')

	#store this challenge and an id

	# return challenge to client
	return jsonify({
			#send challenge id here
			'header' : header_str,
			'target' : target
	})


@app.route('/')
def getData():
	#DEBUG print(request.get_json())
	#DEBUG print( request.get_json()['header'].encode('utf-16')[2:] )
	
	# pull puzzle info from request
	check_header = request.get_json()['header'].encode('utf-16')[2:]
	target = request.get_json()['target']
	# puzzle_id = request.get_json()['id']
	# validate the puzzle
	result = puzzle.validate_puzzle(check_header, target)
	print(result)

	return jsonify({
		'access' : result
	})

# Start app
if __name__ == '__main__':
	app.run()
	print('req')
	#r = requests.post('http://httpbin.org/post', json={"key": "value"})