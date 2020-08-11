from setuptools import setup, find_packages

setup(
	name = 'passgen-py',
	packages = find_packages(), 
	version = '0.1.1',
	description = 'Generate Passwords Deterministically based on a Master Password.',
	classifiers = [
		'Development Status :: 3 - Alpha',
		'License :: OSI Approved :: MIT License',
		'Programming Language :: Python :: 3'
	],
	python_requires='>=3.6, <4',
	entry_points={
        'console_scripts': [
            'passgen=src:cli',
        ],
    },
    install_requires=['click', 'pyperclip'],
)
