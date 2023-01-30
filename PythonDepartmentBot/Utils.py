import os
import sys
import json
import logging
import sqlite3
import discord.ext.commands


class DataUtils:
    def __init__(self, config_path):
        self._config = self.get_config(config_path)
        self._cursor = sqlite3.connect(self._config["database"]).cursor()

    @property
    def config(self):
        return self._config

    @staticmethod
    def get_config(config_path):
        if not os.path.isfile(config_path):
            logging.error(f"{config_path} not found! Please add it and try again.")
            sys.exit()
        else:
            with open(config_path, encoding='cp1251') as file:
                config = json.load(file)
            return config

    def get_links(self):
        try:
            result = self._cursor.execute('SELECT "Name", "Url" FROM "Resources"').fetchall()
        except sqlite3.Error as err:
            logging.error(f"Exception occurred during get_links: {err}")
            return None
        return {tup[0]: tup[1] for tup in result}

    def get_schedule(self):
        try:
            result = self._cursor.execute('SELECT "Group", "Url" FROM "Schedule"').fetchall()
        except sqlite3.Error as err:
            logging.error(f"Exception occurred during get_schedule: {err}")
            return None
        return {str(tup[0]): tup[1] for tup in result}


class RoleUtils:
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
