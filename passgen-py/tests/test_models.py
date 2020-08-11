import json

from src.models import KeyList, Key, Master


def test_keylist_from_json():
    keylist = json.dumps({
        'Master': {
            'Hash': '123abc',
            'Salt': 'saltysalt',
            'IterCount': 10
        },
        'Version': 1,
        'Keys': [{
            'Label': 'key1',
            'GenMode': 'Base64',
            'MaxLength': None
        }, {
            'Label': 'key2',
            'GenMode': 'AlphaNum',
            'MaxLength': 5
        }]
    })
    result = KeyList.from_json(keylist)
    assert type(result.master) is Master
    assert result.master.hashed == '123abc'
    assert result.master.salt == 'saltysalt'
    assert result.master.iter_count == 10

    assert result.version == 1

    assert type(result.keys) is list
    assert len(result.keys) == 2
    assert type(result.keys[0]) is Key
    assert result.keys[0].label == 'key1'
    assert result.keys[0].gen_mode == 'Base64'
    assert result.keys[0].max_length is None
    assert type(result.keys[1]) is Key
    assert result.keys[1].label == 'key2'
    assert result.keys[1].gen_mode == 'AlphaNum'
    assert result.keys[1].max_length == 5


def test_keylist_to_str():
    keylist = json.dumps({
        'Master': {
            'Hash': '123abc',
            'Salt': 'saltysalt',
            'IterCount': 10
        },
        'Version': 1,
        'Keys': [{
            'Label': 'key1',
            'GenMode': 'Base64',
            'MaxLength': None
        }, {
            'Label': 'key2',
            'GenMode': 'AlphaNum',
            'MaxLength': 5
        }]
    })
    result = KeyList.from_json(keylist)
    assert str(result) == keylist
