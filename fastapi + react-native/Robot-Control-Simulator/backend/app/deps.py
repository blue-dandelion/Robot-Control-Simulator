from enum import Enum

available_controls = ["PLACE", "MOVE", "LEFT", "RIGHT", "REPORT"]

class Directions(Enum):
    NORTH = 0
    EAST = 1
    SOUTH = 2
    WEST = 3
    
class Event:
    def __init__(self):
        self.handlers = {}
        
    def add_handler(self, name, func):
        if name not in self.handlers:
            self.handlers[name] = []
        self.handlers[name].append(func)
        
    async def invoke(self, name, *args, **kwargs):
        if name in self.handlers:
            for handler in self.handlers[name]:
                await handler(*args, **kwargs)