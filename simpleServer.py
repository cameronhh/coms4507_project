from flask import Flask, jsonify, request
import requests
from challenges import puzzle
import base64

app = Flask(__name__)



@app.route('/ChallengeMe', methods=['GET', 'POST'])
def getChallenge():
	# generate challenge
	header, target = puzzle.generate_puzzle(16)
	#header_str=header.decode('utf-16')
	header_str= base64.b64encode(header).decode('utf-16')

	#store this challenge and an id

	# return challenge to client
	return jsonify({
			#send challenge id here
			'header' : header_str,
			'target' : target
	})


@app.route('/', methods=['GET', 'POST'])
def getData():
	#DEBUG print(request.get_json())
	#DEBUG print( request.get_json()['header'].encode('utf-16')[2:] )
	
	# pull puzzle info from request
	#check_header = request.get_json()['header'].encode('utf-16')[2:]
	check_header = base64.b64decode(request.get_json()['header'].encode('utf-16')[2:])
		
	target = request.get_json()['target']
	# puzzle_id = request.get_json()['id']
	# validate the puzzle
	result = puzzle.validate_puzzle(check_header, target)
	print('valid puzzle solution received == ' + str(result))
	block_conf_result = puzzle._check_block_confirmation(check_header)
	print('puzzle solution is valid block header == ' + str(block_conf_result))

	return jsonify({
		'access' : result,
		'block_conf' : block_conf_result # for evaluation
	})

# Start app
if __name__ == '__main__':
	app.run()
	print('req')
	#r = requests.post('http://httpbin.org/post', json={"key": "value"})