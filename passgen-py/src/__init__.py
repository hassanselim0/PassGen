from pathlib import Path

import click
import pyperclip

from src import utils
from src.models import Key


@click.group()
def cli():
    utils.get_meta_directory().mkdir(parents=True, exist_ok=True)
    pass


@cli.command()
@click.option(
    '-p', '--master-password',
    prompt=True, hide_input=True, type=str, confirmation_prompt=True)
@click.option(
    '-ic', '--iter-count', prompt=True, type=int, default=1000)
@click.option(
    '-kp', '--keylist-path',
    type=click.Path(), default=utils.get_default_path(),
    prompt=True)
def init(master_password, iter_count, keylist_path):
    if Path(keylist_path).exists():
        click.confirm(
            f'A file already exists at "{keylist_path}". '
            f'Do you want to overwrite it?', abort=True)
    salt = utils.generate_salt()
    hashed = utils.hash_master(master_password, salt, iter_count)
    utils.save_master(keylist_path, hashed, salt, iter_count)
    click.echo(f'Successfully saved new master hash at {keylist_path}!')


@cli.command()
@click.option(
    '-p', '--master-password',
    prompt=True, hide_input=True, type=str)
@click.option(
    '-l', '--label', help='Case-insensitive label for passwords.',
    prompt=True)
@click.option(
    '-kp', '--keylist-path',
    type=click.Path(exists=True),
    default=utils.get_last_used_path(), prompt=True)
def generate(master_password, label, keylist_path):
    keylist = utils.load_keylist(keylist_path)
    abort_if_incorrect_master(master_password, keylist, keylist_path)

    label = label.lower()
    key = keylist.get_key(label)
    if key is None:
        max_length = click.prompt('Max Length', type=int, default=-1)
        max_length = None if max_length <= 0 else max_length

        gen_mode = click.prompt(
            'Gen Mode',
            type=click.Choice(['Base64', 'AlphaNum'], case_sensitive=False),
            default='Base64')
        key = Key(label, gen_mode, max_length)
        keylist.keys.append(key)
    utils.save_keylist(keylist_path, keylist)
    new_password = utils.generate_password(master_password, key)
    pyperclip.copy(new_password)
    click.echo('Password copied to clipboard!')


@cli.command()
@click.option(
    '-p', '--master-password',
    prompt=True, hide_input=True, type=str)
@click.option(
    '-l', '--label', help='Case-insensitive label for passwords.',
    prompt=True)
@click.option(
    '-kp', '--keylist-path',
    type=click.Path(exists=True),
    default=utils.get_last_used_path(), prompt=True)
@click.pass_context
def reset(ctx, master_password, label, keylist_path):
    keylist = utils.load_keylist(keylist_path)
    abort_if_incorrect_master(master_password, keylist, keylist_path)

    label = label.lower()

    keylist.remove_key(label)
    utils.save_keylist(keylist_path, keylist)
    ctx.invoke(generate, master_password, label, keylist_path)


def abort_if_incorrect_master(master_password, keylist, keylist_path):
    if not utils.check_master(master_password, keylist):
        click.echo(
            f'Master password does not match value saved at {keylist_path}.')
        raise click.Abort
