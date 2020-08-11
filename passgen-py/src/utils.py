import base64
from hashlib import pbkdf2_hmac, sha256
import hmac
import json
from secrets import token_bytes
from pathlib import Path

from src.models import KeyList, Master


# hash and check
def check_master(password, keylist):
    hashed = hash_master(
        password, keylist.master.salt, keylist.master.iter_count)
    return hashed == keylist.master.hashed


def hash_master(password, salt, iter_count):
    # salt is assumed to be base64 encoded
    salt = base64.b64decode(salt)
    hashed = pbkdf2_hmac('sha256', password.encode(), salt, iter_count)
    return base64.b64encode(hashed).decode('utf-8')


def generate_password(master, key):
    hashed = hmac.new(
        master.encode(), msg=key.label.encode(), digestmod=sha256).digest()
    hashed = base64.b64encode(hashed).decode('utf-8')

    if key.gen_mode.lower() == 'alphanum':
        hashed = ''.join([c for c in hashed if c.isalnum()])

    if key.max_length is not None:
        hashed = hashed[:key.max_length]
    return hashed


def generate_salt():
    return base64.b64encode(token_bytes(32)).decode('utf-8')


# I/O
def save_master(path, hashed, salt, iter_count):
    # this implies a clean slate
    master = Master(hashed, salt, iter_count)
    keylist = KeyList(master)
    save_keylist(path, keylist)


def save_keylist(path, keylist):
    with open(path, 'w') as f:
        f.write(str(keylist))
    save_keylist_path(path)


def save_keylist_path(path):
    file = get_meta_directory() / 'keylists.meta.json'
    existing = []

    if file.exists():
        with open(str(file)) as f:
            existing = json.loads(f.read()).get('paths', [])

    with open(str(file), 'w') as f:
        if path in existing:
            existing.remove(path)
        existing.append(path)
        f.write(json.dumps({'paths': existing}))


def get_meta_directory():
    return Path.home() / '.passgen'


def get_keylists_meta_path():
    return get_meta_directory() / 'keylists.meta.json'


def get_default_path():
    return str(get_meta_directory() / 'default.keys.json')


def get_last_used_path():
    file = get_keylists_meta_path()

    if file.exists():
        with open(str(file)) as f:
            existing = json.loads(f.read()).get('paths', [])

        if len(existing) > 0:
            return existing[-1]
    return get_default_path()


def load_keylist(path):
    with open(path) as f:
        json_obj = f.read()
    return KeyList.from_json(json_obj)
