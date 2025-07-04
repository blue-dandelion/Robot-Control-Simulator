from deps import Directions, Event
from typing import Literal

class robot:
    def __init__(self, workspace_width: int, workspace_height: int):
        self.workspace_width = workspace_width
        self.workspace_height = workspace_height
        self.pos_x = 0
        self.pos_y = 0
        self.dir = Directions.NORTH
        self.warning_event = Event()
        self.updated_event = Event()
        self.start_moving = False
        self.output = list[str]
        
    async def is_in_workspace(self,x: int, y: int) -> bool:
        if x >= 0 and x < self.workspace_width and y >= 0 and y < self.workspace_height:
            return True
        else:
            await self.warning_event.invoke("")
            return False
    
    async def place(self, x: str, y: str, facing: str):
        new_x = int(x)
        new_y = int(y)
        if not await self.is_in_workspace(new_x, new_y): return
        
        self.pos_x = new_x
        self.pos_y = new_y
        self.dir = Directions[facing]
        self.start_moving = True
        await self.updated_event.invoke("PLACE", {"x":self.pos_x, "y":self.pos_y, "f":self.dir.value})
        
    async def move(self):
        if not self.start_moving: return
        
        new_x = self.pos_x
        new_y = self.pos_y
        
        if self.dir == Directions.NORTH:
            new_y += 1
        elif self.dir == Directions.EAST:
            new_x += 1
        elif self.dir == Directions.SOUTH:
            new_y -= 1
        elif self.dir == Directions.WEST:
            new_x -= 1
        
        if await self.is_in_workspace(new_x, new_y):
            self.pos_x = new_x
            self.pos_y = new_y
            await self.updated_event.invoke("MOVE", {"x":self.pos_x, "y":self.pos_y})       
    
    async def rotate(self, to: Literal["LEFT", "RIGHT"]):
        if not self.start_moving: return
        
        if to == "LEFT":
            self.dir = Directions((self.dir.value - 1) % 4)
        elif to == "RIGHT":
            self.dir = Directions((self.dir.value + 1) % 4)
        await self.updated_event.invoke("ROTATE", self.dir.value)
            
    async def report(self):
        if not self.start_moving: return
        await self.updated_event.invoke("REPORT", f"{self.pos_x},{self.pos_y},{self.dir.name}")