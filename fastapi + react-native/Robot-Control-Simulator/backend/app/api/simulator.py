from models.ide import CodeCompiler
from models.robot import robot
from deps import Event, Directions
import time

class Simulator():
    
    def __init__(self):
        self.updatePreview_event = Event()
        self.warning_event = Event()
        self.error_event = Event()
        self.workspaceWidth = 5
        self.workspaceHeight = 5
    
    def simulate(self, code: str, runLine: bool = False, timespanInSecond: int = 0):
        errors: list[str] = []
        outputs: list[str] = []
        
        #region 1. Set code compiler
        compiler = CodeCompiler()
        compiler.warning_event.add_handler(lambda data: errors.append(f"{data}"))
        #endregion
        
        #region 2. Compile the control code
        code_lines = code.split("\n")
        token_lines = compiler.tokenize(code_lines)
        grammar_correct = compiler.grammar_check(token_lines)

        if not grammar_correct:
            errors.append("Please fix the control code error.")
            return errors, outputs

        #endregion
        
        #region 3. Init a robot
        line_index = 0
        
        rob = robot(self.workspaceWidth, self.workspaceHeight)
        rob.warning_event.add_handler(self.warning_event.invoke(f"(line {line_index + 1})WARNING! Robot falls out of the workspace."))
        
        # Send robot situation to update workspace preview
        rob.updated_event.add_handler("PLACE", on_place)
        def on_place(*args:tuple[int, int, Directions]):
            self.updatePreview_event.invoke("PLACE", f"(line {line_index + 1})Output: {args}")
        rob.updated_event.add_handler("MOVE", on_move)
        def on_move(*args:tuple[int, int]):
            self.updatePreview_event.invoke("MOVE", args)
        rob.updated_event.add_handler("ROTATE", on_rotate)
        def on_rotate(*args:Directions):
            self.updatePreview_event.invoke("ROTATE", args)
        rob.updated_event.add_handler("REPORT", on_report)
        def on_report(*args:str):
            self.updatePreview_event.invoke("REPORT", f"(line {line_index + 1})Output: {args}")
        #endregion
        
        #region 4. Apply code to robot
        for line in token_lines:
            tokens = line
            i = 0
            while i < len(tokens):
                if tokens[i] == "PLACE":
                    info = tokens[i + 1].split(",")
                    rob.place(info[0], info[1], info[2])
                    i += 1
                elif tokens[i] == "MOVE":
                    rob.move()
                elif tokens[i] == "LEFT" or tokens[i] == "RIGHT":
                    rob.rotate(tokens[i])
                elif tokens[i] == "REPORT":
                    rob.report()
                else:
                    break
                i += 1
            line_index += 1
            time.sleep(timespanInSecond)
            
        #endregion

        return errors, outputs