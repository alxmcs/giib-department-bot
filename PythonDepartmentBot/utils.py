import json
import logging
import os
import sqlite3
import threading

import discord

from constants import EVERYONE_ROLE, STUDENT_ROLE


class DataUtils:
    """Thin wrapper around the SQLite database used by the bot.

    A fresh :mod:`sqlite3` connection is opened per query so we can safely
    call into these helpers from discord.py's asyncio callbacks (a single
    shared connection defaults to ``check_same_thread=True`` and would raise
    when reused across the event loop's executor threads).
    """

    def __init__(self, config_path):
        self._config = self.get_config(config_path)
        self._database = self._config["database"]
        self._links_cache = None
        self._links_lock = threading.Lock()

    @property
    def config(self):
        return self._config

    @staticmethod
    def get_config(config_path):
        if not os.path.isfile(config_path):
            # Raising is preferable to ``sys.exit`` from a library helper:
            # the caller gets a chance to handle the missing-config case
            # (e.g. fall back to environment variables).
            raise FileNotFoundError(f"{config_path} not found! Please add it and try again.")
        # JSON is UTF-8 per RFC 8259; the previous cp1251 hard-coding caused
        # decoding failures on any non-Cyrillic-1251 environment.
        with open(config_path, encoding='utf-8') as file:
            return json.load(file)

    def _connect(self):
        # ``check_same_thread=False`` lets the connection be used from
        # discord.py's asyncio executor threads; a single connection per
        # query keeps us on the safe side of SQLite's threading model.
        return sqlite3.connect(self._database, check_same_thread=False)

    def get_links(self):
        """Return a name→url mapping of department resources, cached."""
        if self._links_cache is not None:
            return self._links_cache
        with self._links_lock:
            if self._links_cache is not None:
                return self._links_cache
            try:
                with self._connect() as con:
                    rows = con.execute('SELECT "Name", "Url" FROM "Resources"').fetchall()
            except sqlite3.Error as err:
                logging.error(f"Exception occurred during get_links: {err}")
                return None
            self._links_cache = {name: url for name, url in rows}
            return self._links_cache

    def get_schedule(self, group):
        """Return the schedule URL for ``group`` or ``None`` if absent."""
        if group is None or group == '':
            return None
        try:
            with self._connect() as con:
                row = con.execute(
                    'SELECT "Url" FROM "Schedule" WHERE "Group" = ? LIMIT 1',
                    (group,),
                ).fetchone()
        except sqlite3.Error as err:
            logging.error(f"Exception occurred during get_schedule: {err}")
            return None
        return row[0] if row else None

    def invalidate_cache(self):
        """Test helper — discard the cached links result."""
        self._links_cache = None


class RoleUtils:
    @staticmethod
    def get_role(ctx):
        """Look up the guild role whose name equals the last token of the
        member's nickname (preferred) or username. Returns ``None`` when no
        such role exists."""
        source = ctx.author.nick or ctx.author.name
        if not source:
            return None
        group = source.split()[-1]
        return discord.utils.get(ctx.guild.roles, name=group)

    @staticmethod
    async def set_role(ctx, role):
        """Replace all non-preserved roles with ``role`` in a single API
        call. ``edit(roles=...)`` is atomic, so we avoid the half-applied
        state the previous per-role loop could leave behind."""
        preserved = [r for r in ctx.author.roles if r.name in (STUDENT_ROLE, EVERYONE_ROLE)]
        # Ensure the target role is present without duplicating it
        if role not in preserved:
            preserved.append(role)
        await ctx.author.edit(roles=preserved)

    @staticmethod
    async def set_grad_role(ctx, role):
        """Replace all roles with the graduate role. ``@everyone`` is
        implicit and cannot be removed via the API."""
        await ctx.author.edit(roles=[role])
