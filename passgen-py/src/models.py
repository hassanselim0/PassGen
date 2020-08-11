import json


class KeyList:
    def __init__(self, master, keys=[], version=1):
        self.master = master
        self.version = version
        self.keys = keys

    @classmethod
    def from_json(cls, json_str):
        obj = json.loads(json_str)
        return cls(
            master=Master.from_dict(obj['Master']),
            keys=[Key.from_dict(k) for k in obj.get('Keys', [])],
            version=obj['Version'])

    def get_key(self, label):
        for key in self.keys:
            if key.label == label:
                return key
        return None

    def remove_key(self, label):
        self.keys = [k for k in self.keys if k.label != label]

    def __str__(self):
        obj = {
            "Master": self.master.to_dict(),
            "Version": self.version,
            "Keys": [k.to_dict() for k in self.keys]
        }
        return json.dumps(obj)


class Key:
    def __init__(self, label, gen_mode='Base64', max_length=None):
        self.label = label
        self.gen_mode = gen_mode
        self.max_length = max_length

    @classmethod
    def from_dict(cls, obj):
        return cls(
            obj['Label'],
            gen_mode=obj['GenMode'],
            max_length=obj['MaxLength'])

    def to_dict(self):
        return {
            "Label": self.label,
            "GenMode": self.gen_mode,
            "MaxLength": self.max_length
        }


class Master:
    def __init__(self, hashed, salt, iter_count):
        self.hashed = hashed
        self.salt = salt
        self.iter_count = iter_count

    @classmethod
    def from_dict(cls, obj):
        return cls(
            obj['Hash'],
            obj['Salt'],
            obj['IterCount'])

    def to_dict(self):
        return {
            "Hash": self.hashed,
            "Salt": self.salt,
            "IterCount": self.iter_count
        }
