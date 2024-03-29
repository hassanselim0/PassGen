# PassGen
Generate Passwords Deterministically based on a Master Password

## Motivation
This was started because I had some concerns regarding password managers.
First, there is a small but real chance that I could lose access to all my passwords (temporarily or permanently) if something terrible happened to the service.
Second, the most secure way to store password is not to store them in the first place!

So with this project I designed a way to generated passwords by combining a master password with a "key" (usually website name) via the HMAC-SHA256 cryptographic function.
I then added some convenience features, like storing the list of keys which is displayed as an auto-complete dropdown list, and storing a hash of the master key to do a fail-early check instead of generating an incorrect password and failing the auth attempt later.

You can still use this tool along with a regular password manager (for mobile sync), it would replace the manager's random password generation, and you would still get the benefit of always having access to your passwords regardless of the manager's availability.

## Download
- Windows: Check [Releases Page](https://github.com/hassanselim0/PassGen/releases/latest)
- Linux/MacOS: `pip install passgen-py`

## FAQ
- **Q: Isn't storing a hash of the master password a bad idea?**
- A: It's the old security vs convenience balance, also the hash is generated using salted PBKDF2 HMAC-SHA256 (in v2 keylists). However I do intend to add the ability to disable that convenience.
- **Q: Can I have more than one key list?**
- A: Yes! You can also pass in the key list name as an argument to the executable to launch the tool with that key list pre-selected and loaded. You can then create shortcuts or batch files pinned in start for each of your key lists, this would let allow you to launch the tool very quickly from Search (Win+S).
- **Q: My password needs to be shorter, what do I do?**
- A: Some services have a very small upper limit on password length (which is silly and insecure), you can edit the keylist file and add `"MaxLength": 10` (or any other number) to the relevant key, this will strip the end of the generated password so that it's no longer than the specified length.
- **Q: My password isn't allowed to have symbols, what do I do?**
- A: Some services don't allow symbols in passwords (which is also silly and insecure), you can edit the keylist file and add `"GenMode": "AlphaNum"` to the relevant key, this will generate a password that is Base64 encoded but with all the symbols are removed.
- **Q: My password needs to have symbols, what do I do?**
- A: Some services require you to add at least one symbol to the password (and for some reason they don't consider `=` to be a symbol), so as of version 1.2 you can edit the keylist file and add `"GenMode": "Base64WithSymbol"` to the relevant key, this will generate a password that is Base64 encoded and has an exclamation mark at the end (sounds unsafe, but it's those damn unreasonable password rules that are actually unsafe!).
- **Q: What if I need to change my password?**
- A: In you need to change to a newer password for the same service (eg: the password got leaked or the service detects suspicious logins). You can create a new key for it, or (as of version 1.2) you can edit the keylist file and add `"PasswordChanges": 1` (or any higher number) to the relevant key, this number will be concatenated to the key's label during password generation. And from now on you'll be generating a new password for that key.
- **Q: How can I synchronize the key list?**
- A: This decision is left for the user, depending on how you want to sacrifice security for convenience, you can go from manual sync via encrypted flash drive to auto-sync with Google Drive / OneDrive / Dropbox. If I make it possible to disable storing of the master hash, then the key file would effectively contain no critical secrets.
- **Q: What are Keylist Versions?**
- A: v0 was a text-based file that didn't allow much flexibility, v1 moved to JSON files and used repeated salted SHA256 for master pass hash, v2 uses proper PBKDF2 HMAC-SHA256 for master pass hash.
- **Q: Android Version?**
- A: I initially wanted to build an Android version for this tool, but I gave up and used a password manager for the few passwords that I use on my phone, feel free to build your own and ping me.
- **Q: Linux Version?**
- A: Yes! Thanks to @mariamrf there is now a Python CLI version of PassGen that can run anywhere where Python runs!
- **Q: This code is ugly and doesn't use MVVM!**
- A: This is a hobby-project initially made for personal use, very few hours were put into the first version of this.
- **Q: I don't see a value in this tool.**
- A: Just use a full-blown Password Manager, much better than nothing. Stop re-using passwords, and stop inventing simple password generation formulas in your head.

## Contributing
I can't guarantee I'll be very responsive to issues and pull requests. But I'd be interested in discussing improvements to this project and possibly accept pull requests.

## License
This project is licensed under the [MIT License](LICENSE)... honestly I'm not quite aware of the differences between this and other popular licenses, so that was pretty much a random decision.