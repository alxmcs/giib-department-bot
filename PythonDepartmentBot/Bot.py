# сооружено по документации https://discordpy.readthedocs.io/en/stable/api.html

import os
import platform
import logging
from discord import Intents
from discord.ext.commands import Bot
import discord.ext.commands
from Utils import DataUtils, RoleUtils
import sys

CONFIG_PATH = 'config.json'
GRADUATE_ROLES = ['6511', '6512', '6513', '6514']

logging.basicConfig(stream=sys.stdout, level=logging.INFO)
data_utils = DataUtils(CONFIG_PATH)
bot = Bot(command_prefix=f"{data_utils.config['prefix']} ", intents=Intents.all())


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
    full_command_name = context.command.qualified_name
    split = full_command_name.split(" ")
    executed_command = str(split[0])
    logging.info(
        f"Tried executing {executed_command} command in {context.guild.name} (ID: {context.message.guild.id}) by {context.message.author} (ID: {context.message.author.id}) but it errored: {error}")


@bot.event
async def on_guild_available(guild):
    """
    The code in this event is executed every time a guild becomes avaliable
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
    schedule = data_utils.get_schedule()
    role = RoleUtils.get_role(ctx)
    if role is None:
        await ctx.send('Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*')
    else:
        if role.name in schedule.keys():
            await ctx.send(schedule[role.name])
        else:
            await ctx.send(f'Для группы {role.name} расписания не нашлось')


@bot.command(name='links', description='Выдает ссылки на информационные ресурсы кафедры')
async def send_links(ctx):
    links = data_utils.get_links()
    message = 'Информационные ресурсы кафедры ГИиИБ:\n'
    for key in links.keys():
        message += f"{key}\n<{links[key]}\n>"
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
    bot.run(data_utils.config["token"])
