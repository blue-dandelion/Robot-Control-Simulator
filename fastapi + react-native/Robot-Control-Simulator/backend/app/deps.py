from enum import Enum

available_controls = ["PLACE", "MOVE", "LEFT", "RIGHT", "REPORT"]

class Directions(Enum):
    NORTH = 0
    EAST = 1
    SOUTH = 2
    WEST = 3
    
class Event:
    def __init__(self):
        self.handlers = []
        
    def add_handler(self, func):
        self.handlers.append(func)
        
    def invoke(self, *args, **kwargs):
        for handler in self.handlers:
            handler(*args, **kwargs)