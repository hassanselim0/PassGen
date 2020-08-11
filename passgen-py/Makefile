bootstrap:
	@pipenv install --dev

clean:
	@find . -type f -name '*.pyc' -delete
	@find . -type d -name '__pycache__' -delete

lint:
	@pipenv run flake8 src tests

test:
	make clean
	make lint
	@pipenv run py.test tests --cov=src
