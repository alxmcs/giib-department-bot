"""Shared role-name constants for the Python department bot.

Keeping these in a single module prevents the magic strings ``Студент`` /
``@everyone`` / ``Выпускник`` from leaking into the command handlers and keeps
the Python implementation in sync with the C# one (see ``BotConstants.cs``).
"""

STUDENT_ROLE = "Студент"
GRADUATE_ROLE = "Выпускник"
EVERYONE_ROLE = "@everyone"

DEFAULT_GRADUATE_GROUPS = ("6511", "6512", "6513", "6514")
