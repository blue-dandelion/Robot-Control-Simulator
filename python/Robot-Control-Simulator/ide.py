from enum import Enum

available_controls = ["PLACE", "MOVE", "LEFT", "RIGHT", "REPORT"]

class Directions(Enum):
    NORTH = 0
    EAST = 1
    SOUTH = 2
    WEST = 3

def tokenize(code_lines: list[str]) -> list[list[str]]:
    tokens = []
    
    for lines in code_lines:
        line_tokens = []
        for word in lines.split():
            line_tokens.append(word)
        tokens.append(line_tokens)
            
    return tokens

def grammar_check(token_lines: list[list[str]]) -> bool:
    if len(token_lines) == 0: return False
    
    for line_index, line in enumerate(token_lines):
        for token_index, token in enumerate(line):
            # Check if the token is available controls
            if token not in available_controls:
                # It is PLACE line
                if line[token_index - 1] == "PLACE":
                    # Check if there is essential information for PLACE
                    parts = token.split(",")
                    if len(parts) == 3 and parts[0].isdigit() and parts[1].isdigit() and parts[2] in Directions.__members__:
                        continue
                    else:                
                        print(f"ERROR! (line {line_index})Invalie info for PLACE: {token}")
                        return False 
                # It is not PLACE line
                else:             
                    print(f"ERROR! (line {line_index})Invalie token: {token}")
                    return False
            else: 
                continue
    return True