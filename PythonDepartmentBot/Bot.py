# сооружено по документации https://discordpy.readthedocs.io/en/stable/api.html

import json
import os
import platform
import sys
import discord
from datetime import datetime
from discord.ext.commands import Bot
import discord.ext.commands 

def get_config():
    if not os.path.isfile("config.json"):
        sys.exit("'config.json' not found! Please add it and try again.")
    else:
        with open("config.json", encoding='cp1251') as file:
            config = json.load(file)
        return config

def get_links():
    if not os.path.isfile("links.json"):
         print(f"{datetime.now()}: 'links.json' not found!")
    else:
        with open("links.json", encoding='cp1251') as file:
            links = json.load(file)
        return links

def get_schedule():
    if not os.path.isfile("schedule.json"):
        print(f"{datetime.now()}: 'schedule.json' not found!")
    else:
        with open("schedule.json", encoding='cp1251') as file:
            schedule = json.load(file)
        return schedule

def get_role(ctx):
    name_group = ctx.author.name.split()[-1]
    nick_group = ctx.author.nick.split()[-1]
    name_role = discord.utils.get(ctx.guild.roles,name=name_group)
    nick_role = discord.utils.get(ctx.guild.roles,name=nick_group)
    if nick_role is not None:
        return nick_role
    if name_role is not None:
        return name_role
    return None

async def set_role(ctx, role):
    for r in ctx.author.roles:
        if r.name != "Студент" and r.name != "@everyone":
            await ctx.author.remove_roles(r)
    await ctx.author.add_roles(role)

config = get_config()
bot = Bot(config["prefix"]+" ") # почему в C# после префикса при парсинге команды подразумевается пробел между префиксом и именем команды, а тут нет - в душе не ебу

@bot.event
async def on_ready():
    """
    The code in this even is executed when the bot is ready
    """
    print(f"{datetime.now()}: Logged in as {bot.user.name}")
    print(f"{datetime.now()}: discord API version: {discord.__version__}")
    print(f"{datetime.now()}: Python version: {platform.python_version()}")
    print(f"{datetime.now()}: Running on: {platform.system()} {platform.release()} ({os.name})")
@bot.event
async def on_command_completion(context):
        """
        The code in this event is executed every time a normal command has been *successfully* executed
        :param context: The context of the command that has been executed.
        """
        full_command_name = context.command.qualified_name
        split = full_command_name.split(" ")
        executed_command = str(split[0])
        print(f"{datetime.now()}: Executed {executed_command} command in {context.guild.name} (ID: {context.message.guild.id}) by {context.message.author} (ID: {context.message.author.id})")
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
        print(f"{datetime.now()}: Tried executing {executed_command} command in {context.guild.name} (ID: {context.message.guild.id}) by {context.message.author} (ID: {context.message.author.id}) but it errored: {error}")
@bot.event
async def on_guild_available(guild):
        """
        The code in this event is executed every time a guild becomes avaliable
        """
        print(f"{datetime.now()}: Guild available: {guild.name}")


@bot.command(name = 'role', description = 'Присваивает роль студенту в соответствии с его никнеймом')
async def grant_role(ctx):
    role = get_role(ctx)
    if role is None:
        await ctx.send('Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*')
    else:
        await set_role(ctx,role)
        await ctx.send(f'Теперь ты в группе {role.name}!')

@bot.command(name = 'schedule', description = 'Выдает ссылку на расписание группы студента в соответствии с его группой')
async def send_schedule(ctx):
    schedule = get_schedule()
    role = get_role(ctx)
    if role is None:
        await ctx.send('Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*')
    else:
        if role.name in schedule.keys():
            await ctx.send(schedule[role.name])
        else:
            await ctx.send(f'Для группы {role.name} расписания не нашлось')

@bot.command(name = 'links', description = 'Выдает ссылки на информационные ресурсы кафедры')
async def send_links(ctx):
    links = get_links()
    message = 'Информационные ресурсы кафедры ГИиИБ:\n'
    for key in links.keys():
        message += f"{key}\n<{links[key]}>"
    await ctx.send(message)

bot.run(config["token"])