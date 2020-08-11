from base64 import b64encode

import pytest

from src import utils
from src.models import KeyList, Master, Key


def test_check_master_matches():
    master_pass = '1234abc'
    some_salt = b64encode('saltysalt'.encode()).decode('utf-8')
    iter_count = 1000
    keylist = KeyList(
        Master(
            utils.hash_master(master_pass, some_salt, iter_count),
            some_salt,
            iter_count))
    assert utils.check_master(master_pass, keylist)


def test_check_master_does_not_match():
    master_pass = '1234abc'
    other_pass = '1234abd'
    some_salt = b64encode('saltysalt'.encode()).decode('utf-8')
    iter_count = 1000
    keylist = KeyList(
        Master(
            utils.hash_master(master_pass, some_salt, iter_count),
            some_salt,
            iter_count))
    assert not utils.check_master(other_pass, keylist)


def test_generate_password_max_length_none_works():
    key = Key('somelabel')
    master = '1234abc'

    assert len(utils.generate_password(master, key)) > 0


def test_generate_password_max_length_specified():
    max_length = 3
    key = Key('somelabel', max_length=max_length)
    master = '1234abc'

    assert len(utils.generate_password(master, key)) == max_length


def test_generate_password_max_length_super_big():
    max_length = 9000
    key = Key('somelabel', max_length=max_length)
    master = '1234abc'

    assert len(utils.generate_password(master, key)) < max_length


def test_generate_password_gen_mode_alphanum():
    key = Key('somelabel', gen_mode='alphanum')
    master = '1234abc'

    assert all(
        map(lambda x: x.isalnum(), utils.generate_password(master, key)))


def test_load_keylist():
    fixture = 'tests/fixtures/test_keylist.keys.json'
    result = utils.load_keylist(fixture)

    assert type(result) is KeyList
    assert len(result.keys) == 3
    assert type(result.master.hashed) is str
    assert len(result.master.hashed) > 0
    assert type(result.master.salt) is str
    assert len(result.master.salt) > 0
    assert result.master.iter_count == 1000


def test_load_keylist_nonexistent():
    fixture = 'tests/fixtures/does/not/exist.json'

    with pytest.raises(FileNotFoundError):
        utils.load_keylist(fixture)
