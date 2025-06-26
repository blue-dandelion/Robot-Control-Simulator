from models.ide import CodeCompiler
from models.robot import robot

def simulate(code: str) -> tuple[list[str], list[str]]:
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

    #region 3. Apply code to robot
    line_index = 0

    rob = robot(5, 5)
    rob.warning_event.add_handler(lambda: errors.append(f"(line {line_index + 1})WARNING! Robot falls out of the workspace."))
    rob.report_event.add_handler(lambda: outputs.append(f"(line {line_index + 1})Output: {rob.pos_x},{rob.pos_y},{rob.dir.name}"))

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
    #endregion

    return errors, outputs