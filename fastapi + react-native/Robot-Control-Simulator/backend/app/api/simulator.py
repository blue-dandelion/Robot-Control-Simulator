from models.ide import CodeCompiler
from models.robot import robot
from deps import Event
import asyncio

class Simulator():
    
    def __init__(self):
        self.updatePreview_event = Event()
        self.warning_event = Event()
        self.error_event = Event()
        self.workspaceWidth = 5
        self.workspaceHeight = 5
    
    async def simulate(self, code: str, runLine: bool = False, timespanInSecond: int = 0):
        #region 1. Set code compiler
        compiler = CodeCompiler()
        
        async def compiler_on_warning(*args):
            await self.warning_event.invoke("", args)
        compiler.warning_event.add_handler("", compiler_on_warning)
        #endregion
        
        #region 2. Compile the control code
        code_lines = code.split("\n")
        token_lines = compiler.tokenize(code_lines)
        grammar_correct = await compiler.grammar_check(token_lines)

        if not grammar_correct:
            return
        #endregion
        
        #region 3. Init a robot
        line_index = 0
        
        rob = robot(self.workspaceWidth, self.workspaceHeight)
        async def rob_on_warning(*args):
            await self.warning_event.invoke("", f"(line {line_index + 1})WARNING! Robot falls out of the workspace.")
        rob.warning_event.add_handler("", rob_on_warning)
        
        # Send robot situation to update workspace preview
        async def on_place(*args):
            await self.updatePreview_event.invoke("PLACE", *args)
        rob.updated_event.add_handler("PLACE", on_place)
            
        async def on_move(*args):
            await self.updatePreview_event.invoke("MOVE", *args)
        rob.updated_event.add_handler("MOVE", on_move)
            
        async def on_rotate(*args):
            await self.updatePreview_event.invoke("ROTATE", *args)
        rob.updated_event.add_handler("ROTATE", on_rotate)
            
        async def on_report(*args):
            await self.updatePreview_event.invoke("REPORT", f"(line {line_index + 1})Output: {args}")
        rob.updated_event.add_handler("REPORT", on_report)
        #endregion
        
        #region 4. Apply code to robot
        for line in token_lines:
            tokens = line
            i = 0
            while i < len(tokens):
                if tokens[i] == "PLACE":
                    info = tokens[i + 1].split(",")
                    await rob.place(info[0], info[1], info[2])
                    i += 1
                elif tokens[i] == "MOVE":
                    await rob.move()
                elif tokens[i] == "LEFT" or tokens[i] == "RIGHT":
                    await rob.rotate(tokens[i])
                elif tokens[i] == "REPORT":
                    await rob.report()
                else:
                    break
                i += 1
            line_index += 1
            await asyncio.sleep(timespanInSecond)
        #endregion