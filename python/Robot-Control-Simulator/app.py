from fileIO import import_file
from ide import tokenize, grammar_check
from robot import robot

workspace_width = 5
workspace_height = 5

#region 1. Get the robot control code
control_code = []
while True:
    print("Enter the control code of the robot\n(press f to input code from file, press e to manually input)")
    choice = input()

    if(choice == "f"):
        read_from_file_suceed = False
        
        while True:
            print("(press e to switch to manual enter)")
            enter = input("Enter file path:")
            if enter == "e":
                break
            else:
                file_content = import_file(enter)
                if file_content == None:
                    print(f"ERROR! File path not exists: {enter}")
                    continue
                else: 
                    control_code = file_content.split("\n")
                    read_from_file_suceed = True
                    break
        if read_from_file_suceed:
            break
        else:
            continue
            
    elif(choice == "e"):
        print("(press Enter on an empty line to finish):")
        while True:
            enter = input()
            if enter == "": break
            control_code.append(enter)
        break
    else:
        continue
#endregion

#region 2. compile the control code
token_lines = tokenize(control_code)
grammar_correct = grammar_check(token_lines)

if not grammar_correct:
    print("Please fix the control code error.")
    exit()
#endregion

#region 3. Apply control code to robot simulator
line_id = 0

rob = robot(5, 5)
rob.warning_event.setdefault("danger", lambda: print(f"WARNING! (line {line_id})Robot falls out of the workspace."))

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
    line_id += 1
#endregion
