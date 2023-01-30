import os
import json
import sqlite3
import tempfile
import unittest
from utils import DataUtils


class DataUtilsTestCase(unittest.TestCase):

    def setUp(self):
        if not os.path.exists(os.path.join(os.path.dirname(__file__), 'temp')):
            os.mkdir(os.path.join(os.path.dirname(__file__), 'temp'))
        self._test_dir = tempfile.TemporaryDirectory(dir=os.path.join(os.path.dirname(__file__), 'temp'))
        self._config_path = self.set_config()
        self._connection = self.set_database()
        self.data_utils = DataUtils(self._config_path)
        self.data_utils._cursor = self._connection.cursor()

    def set_config(self):
        path = os.path.join(self._test_dir.name, 'test.json')
        test_config = {"token": "test_token",
                       "prefix": "!test_prefix",
                       "database": ':memory:'}
        with open(path, 'w') as config:
            json.dump(test_config, config)
        return path

    @staticmethod
    def set_database():
        connection = sqlite3.connect(':memory:')
        cur = connection.cursor()
        cur.execute('CREATE TABLE "Resources"("Id" INTEGER NOT NULL UNIQUE, "Name" TEXT, "Url" TEXT,PRIMARY KEY("Id" AUTOINCREMENT));')
        cur.execute('CREATE TABLE "Schedule"("Id" INTEGER NOT NULL UNIQUE,"Group" INTEGER,"Url"	TEXT,PRIMARY KEY("Id" AUTOINCREMENT));')
        cur.execute('INSERT INTO "Resources"("Name", "Url") VALUES (\'TestName1\',\'TestUrl1\'),(\'TestName2\',\'TestUrl2\');')
        cur.execute('INSERT INTO "Schedule"("Group", "Url") VALUES (\'1111\',\'TestUrl1\'),(\'1112\',\'TestUrl2\');')
        connection.commit()
        cur.close()
        return connection

    def tearDown(self):
        self._connection.close()
        self._test_dir.cleanup()

    def test_get_config(self):
        config = self.data_utils.get_config(self._config_path)
        self.assertIsNotNone(config)
        self.assertTrue("token" in config.keys())
        self.assertIsNotNone(config["token"])
        self.assertEqual("test_token", config["token"])
        self.assertTrue("prefix" in config.keys())
        self.assertIsNotNone(config["prefix"])
        self.assertEqual("!test_prefix", config["prefix"])
        self.assertTrue("database" in config.keys())
        self.assertIsNotNone(config["database"])
        self.assertEqual(':memory:', config["database"])

    def test_get_links(self):
        links = self.data_utils.get_links()
        self.assertIsNotNone(links)
        self.assertTrue("TestName1" in links.keys())
        self.assertIsNotNone(links["TestName1"])
        self.assertEqual("TestUrl1", links["TestName1"])
        self.assertTrue("TestName2" in links.keys())
        self.assertIsNotNone(links["TestName2"])
        self.assertEqual("TestUrl2", links["TestName2"])

    def test_get_schedule(self):
        schedule = self.data_utils.get_schedule()
        self.assertIsNotNone(schedule)
        self.assertTrue("1111" in schedule.keys())
        self.assertIsNotNone(schedule["1111"])
        self.assertEqual("TestUrl1", schedule["1111"])
        self.assertTrue("1112" in schedule.keys())
        self.assertIsNotNone(schedule["1112"])
        self.assertEqual("TestUrl2", schedule["1112"])
