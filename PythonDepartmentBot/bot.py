# сооружено по документации https://discordpy.readthedocs.io/en/stable/api.html

import logging
import os
import platform
import sys

import discord
from discord import Intents
from discord.ext.commands import Bot

from constants import DEFAULT_GRADUATE_GROUPS
from utils import DataUtils, RoleUtils

CONFIG_PATH = 'config.json'

logging.basicConfig(stream=sys.stdout, level=logging.INFO)

data_utils = DataUtils(CONFIG_PATH)
# Graduate groups live in config so the Python and C# implementations share a
# single source of truth (falling back to the package defaults for older
# configs).
GRADUATE_ROLES = list(data_utils.config.get('graduate_groups', DEFAULT_GRADUATE_GROUPS))

# Minimum set of intents required by the bot. ``message_content`` is a
# privileged intent but is needed for prefix commands; ``members`` is needed
# for the on-member-join handler.
_intents = Intents.none()
_intents.guilds = True
_intents.members = True
_intents.guild_messages = True
_intents.message_content = True

bot = Bot(command_prefix=f"{data_utils.config['prefix']} ", intents=_intents)


@bot.event
async def on_ready():
    """
    The code in this even is executed when the bot is ready
    """
    logging.info(f"Logged in as {bot.user.name}")
    logging.info(f"discord API version: {discord.__version__}")
    logging.info(f"Python version: {platform.python_version()}")
    logging.info(f"Running on: {platform.system()} {platform.release()} ({os.name})")
    logging.info(f"Running with command prefix: {data_utils.config['prefix']}")


@bot.event
async def on_command_completion(context):
    """
    The code in this event is executed every time a normal command has been *successfully* executed
    :param context: The context of the command that has been executed.
    """
    full_command_name = context.command.qualified_name
    split = full_command_name.split(" ")
    executed_command = str(split[0])
    logging.info(
        f"Executed {executed_command} command in {context.guild.name} (ID: {context.message.guild.id}) by {context.message.author} (ID: {context.message.author.id})")


@bot.event
async def on_command_error(context, error):
    """
    The code in this event is executed every time a normal valid command catches an error
    :param context: The normal command that failed executing.
    :param error: The error that has been faced.
    """
    command_name = context.command.qualified_name if context.command else "<unknown command>"
    executed_command = command_name.split(" ")[0]
    guild_name = context.guild.name if context.guild else "<no guild>"
    guild_id = context.message.guild.id if context.message.guild else "?"
    logging.info(
        f"Tried executing {executed_command} command in {guild_name} (ID: {guild_id}) by {context.message.author} (ID: {context.message.author.id}) but it errored: {error}")


@bot.event
async def on_guild_available(guild):
    """
    The code in this event is executed every time a guild becomes available
    """
    logging.info(f"Guild available: {guild.name}")


@bot.command(name='role', description='Присваивает роль студенту в соответствии с его никнеймом')
async def grant_role(ctx):
    role = RoleUtils.get_role(ctx)
    if role is None:
        await ctx.send('Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*')
    else:
        await RoleUtils.set_role(ctx, role)
        await ctx.send(f'Теперь ты в группе {role.name}!')


@bot.command(name='schedule', description='Выдает ссылку на расписание группы студента в соответствии с его группой')
async def send_schedule(ctx):
    role = RoleUtils.get_role(ctx)
    if role is None:
        await ctx.send('Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*')
        return
    url = data_utils.get_schedule(role.name)
    if url:
        await ctx.send(url)
    else:
        await ctx.send(f'Для группы {role.name} расписания не нашлось')


@bot.command(name='links', description='Выдает ссылки на информационные ресурсы кафедры')
async def send_links(ctx):
    links = data_utils.get_links() or {}
    message = 'Информационные ресурсы кафедры ГИиИБ:\n'
    for name, url in links.items():
        message += f"{name}\n<{url}>\n"
    await ctx.send(message)


@bot.command(name='graduate', description='Присваивает студенту последнего курса роль выпускника')
async def graduate(ctx):
    role = RoleUtils.get_role(ctx)
    if role and role.name in GRADUATE_ROLES:
        await RoleUtils.set_grad_role(ctx, role)
        await ctx.send(f'Теперь {role.name}!')
    else:
        await ctx.send('Ты не на последнем курсе!')


if __name__ == "__main__":
    # Prefer the ``DISCORD_TOKEN`` environment variable so container
    # deployments don't need a tokenised config file on disk.
    token = os.environ.get('DISCORD_TOKEN') or data_utils.config.get('token')
    if not token:
        raise RuntimeError(
            "Discord token is not configured. Set the DISCORD_TOKEN environment "
            "variable or provide \"token\" in config.json."
        )
    bot.run(token)
