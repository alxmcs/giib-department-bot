import json
import os
import sqlite3
import tempfile
import unittest

from utils import DataUtils


class DataUtilsTestCase(unittest.TestCase):

    def setUp(self):
        if not os.path.exists(os.path.join(os.path.dirname(__file__), 'temp')):
            os.mkdir(os.path.join(os.path.dirname(__file__), 'temp'))
        self._test_dir = tempfile.TemporaryDirectory(dir=os.path.join(os.path.dirname(__file__), 'temp'))
        # Use a real SQLite file so the connect-per-query strategy in
        # DataUtils sees the same database across calls (in-memory dbs are
        # per-connection).
        self._db_path = os.path.join(self._test_dir.name, 'test.db')
        self.seed_database(self._db_path)
        self._config_path = self.set_config(self._db_path)
        self.data_utils = DataUtils(self._config_path)

    def set_config(self, db_path):
        path = os.path.join(self._test_dir.name, 'test.json')
        test_config = {"token": "test_token",
                       "prefix": "!test_prefix",
                       "database": db_path}
        with open(path, 'w', encoding='utf-8') as config:
            json.dump(test_config, config)
        return path

    @staticmethod
    def seed_database(db_path):
        connection = sqlite3.connect(db_path)
        cur = connection.cursor()
        cur.execute('CREATE TABLE "Resources"("Id" INTEGER NOT NULL UNIQUE, "Name" TEXT, "Url" TEXT,PRIMARY KEY("Id" AUTOINCREMENT));')
        cur.execute('CREATE TABLE "Schedule"("Id" INTEGER NOT NULL UNIQUE,"Group" INTEGER,"Url"	TEXT,PRIMARY KEY("Id" AUTOINCREMENT));')
        cur.execute('INSERT INTO "Resources"("Name", "Url") VALUES (\'TestName1\',\'TestUrl1\'),(\'TestName2\',\'TestUrl2\');')
        cur.execute('INSERT INTO "Schedule"("Group", "Url") VALUES (\'1111\',\'TestUrl1\'),(\'1112\',\'TestUrl2\');')
        connection.commit()
        cur.close()
        connection.close()

    def tearDown(self):
        self._test_dir.cleanup()

    def test_get_config(self):
        config = self.data_utils.get_config(self._config_path)
        self.assertIsNotNone(config)
        self.assertIn("token", config)
        self.assertEqual("test_token", config["token"])
        self.assertIn("prefix", config)
        self.assertEqual("!test_prefix", config["prefix"])
        self.assertIn("database", config)
        self.assertEqual(self._db_path, config["database"])

    def test_get_config_missing_file_raises(self):
        with self.assertRaises(FileNotFoundError):
            DataUtils.get_config(os.path.join(self._test_dir.name, 'does-not-exist.json'))

    def test_get_links(self):
        links = self.data_utils.get_links()
        self.assertIsNotNone(links)
        self.assertEqual("TestUrl1", links.get("TestName1"))
        self.assertEqual("TestUrl2", links.get("TestName2"))

    def test_get_links_is_cached(self):
        # Second call should return the exact same object from the cache.
        first = self.data_utils.get_links()
        second = self.data_utils.get_links()
        self.assertIs(first, second)

    def test_get_schedule(self):
        self.assertEqual("TestUrl1", self.data_utils.get_schedule("1111"))
        self.assertEqual("TestUrl2", self.data_utils.get_schedule("1112"))

    def test_get_schedule_missing_group_returns_none(self):
        self.assertIsNone(self.data_utils.get_schedule("9999"))

    def test_get_schedule_empty_returns_none(self):
        self.assertIsNone(self.data_utils.get_schedule(""))
        self.assertIsNone(self.data_utils.get_schedule(None))
