from api.ide import tokenize, grammar_check
from api.robot import robot

def simulate(code: str) -> tuple[list[str], list[str]]:
    errors: list[str] = {}
    outputs: list[str] = {}
    
    errors.append("hahaha")
    #region 1. Seperate codes into lines
    code_lines = code.split("\n")
    #endregion
    
    #region 2. Compile the control code
    token_lines = tokenize(code_lines)
    grammar_correct = grammar_check(token_lines)

    if not grammar_correct:
        print("Please fix the control code error.")
        exit()
    #endregion

    #region 3. Apply code to robot
    line_index = 0

    rob = robot(5, 5)
    rob.warning_event.setdefault("danger", lambda: errors.append(f"WARNING! (line {line_index + 1})Robot falls out of the workspace."))
    rob.warning_event.setdefault("report", lambda: outputs.append(f"(line {line_index + 1})Output: {rob.pos_x},{rob.pos_y},{rob.dir.name}"))

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