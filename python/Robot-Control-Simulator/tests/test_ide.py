import unittest, robot_control_simulator.ide as ide

input = ["MOVE", "PLACE 0,0,NORTH", "LEFT", "MOVE", "RIGHT", "MOVE", "REPORT"]

expect_output_tokenize = [["MOVE"], ["PLACE", "0,0,NORTH"], ["LEFT"], ["MOVE"], ["RIGHT"], ["MOVE"], ["REPORT"]]

class TestExample(unittest.TestCase):
    
    @classmethod
    def setUpClass(cls):
        print('Starting Test Case')
        
    @classmethod
    def tearDownClass(cls):
        print('Test Case Ended')
    
    def setUp(self):
        print('Set Up')
        
    def tearDown(self):
        print('Tear Down')
    
    def test_ide(self):
        output_tokenize = ide.tokenize(input)
        with self.subTest("Tokenize"):
            self.assertEqual(output_tokenize, expect_output_tokenize)
        
        output_grammar_check = ide.grammar_check(output_tokenize)
        with self.subTest("Grammar Check"):
            self.assertTrue(output_grammar_check)