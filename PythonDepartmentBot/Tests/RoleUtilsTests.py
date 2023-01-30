import unittest
from enum import Enum
from Utils import RoleUtils
from unittest.mock import MagicMock


class ContextMockType(Enum):
    NO_NAME = 0
    NAME = 1
    NICK = 2
    BOTH = 3
    WRONG = 4


def create_mock_role():
    mock1 = MagicMock()
    mock1.name = '1111'
    mock2 = MagicMock()
    mock2.name = '1112'
    return [mock1, mock2]


def create_mock_context(mock_type):
    mock = MagicMock()
    match mock_type:
        case ContextMockType.NO_NAME:
            mock.author.nick = None
            mock.author.name = None
        case ContextMockType.NAME:
            mock.author.nick = None
            mock.author.name = 'Test Name 1111'
            mock.guild.roles = create_mock_role()
        case ContextMockType.NICK:
            mock.author.nick = 'Test Name 1112'
            mock.author.name = None
            mock.guild.roles = create_mock_role()
        case ContextMockType.BOTH:
            mock.author.nick = 'Test Name 1112'
            mock.author.name = 'Test Name 1111'
            mock.guild.roles = create_mock_role()
        case ContextMockType.WRONG:
            mock.author.nick = 'Test Name 1113'
            mock.guild.roles = create_mock_role()
    return mock


class RoleUtilsTestCase(unittest.TestCase):

    def test_get_role_no_name(self):
        result = RoleUtils.get_role(create_mock_context(ContextMockType.NO_NAME))
        self.assertIsNone(result)

    def test_get_role_name(self):
        result = RoleUtils.get_role(create_mock_context(ContextMockType.NAME))
        self.assertIsNotNone(result)
        self.assertIsNotNone(result.name)
        self.assertEqual('1111', result.name)

    def test_get_role_nick(self):
        result = RoleUtils.get_role(create_mock_context(ContextMockType.NICK))
        self.assertIsNotNone(result)
        self.assertIsNotNone(result.name)
        self.assertEqual('1112', result.name)

    def test_get_role_both(self):
        result = RoleUtils.get_role(create_mock_context(ContextMockType.BOTH))
        self.assertIsNotNone(result)
        self.assertIsNotNone(result.name)
        self.assertEqual('1112', result.name)

    def test_get_role_wrong_roles(self):
        result = RoleUtils.get_role(create_mock_context(ContextMockType.WRONG))
        self.assertIsNone(result)
