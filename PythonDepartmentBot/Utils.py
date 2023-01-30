import os
import sys
import json
import logging
import discord.ext.commands


class BotUtils:
    @staticmethod
    def get_config():
        if not os.path.isfile("config.json"):
            logging.error("'config.json' not found! Please add it and try again.")
            sys.exit()
        else:
            with open("config.json", encoding='cp1251') as file:
                config = json.load(file)
            return config

    @staticmethod
    def get_links():
        if not os.path.isfile("links.json"):
            logging.info("'links.json' not found!")
        else:
            with open("links.json", encoding='cp1251') as file:
                links = json.load(file)
            return links

    @staticmethod
    def get_schedule():
        if not os.path.isfile("schedule.json"):
            logging.info("'schedule.json' not found!")
        else:
            with open("schedule.json", encoding='cp1251') as file:
                schedule = json.load(file)
            return schedule

    @staticmethod
    def get_role(ctx):
        if ctx.author.nick:
            nick_group = ctx.author.nick.split()[-1]
            nick_role = discord.utils.get(ctx.guild.roles, name=nick_group)
            return nick_role
        elif ctx.author.name:
            name_group = ctx.author.name.split()[-1]
            name_role = discord.utils.get(ctx.guild.roles, name=name_group)
            return name_role
        return None

    @staticmethod
    async def set_role(ctx, role):
        for r in ctx.author.roles:
            if r.name != "Студент" and r.name != "@everyone":
                await ctx.author.remove_roles(r)
        await ctx.author.add_roles(role)

    @staticmethod
    async def set_grad_role(ctx, role):
        for r in ctx.author.roles:
            if r.name != "@everyone":
                await ctx.author.remove_roles(r)
        await ctx.author.add_roles(role)