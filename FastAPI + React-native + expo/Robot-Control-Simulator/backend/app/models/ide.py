from deps import Directions,Event, available_controls

class CodeCompiler:
    
    def __init__(self):
        self.warning_event = Event()

    def tokenize(self, code_lines: list[str]) -> list[list[str]]:
        tokens = []
        
        for lines in code_lines:
            line_tokens = []
            for word in lines.split():
                line_tokens.append(word)
            tokens.append(line_tokens)
                
        return tokens

    async def grammar_check(self, token_lines: list[list[str]]) -> bool:
        if len(token_lines) == 0: return False
        
        start_moving = False
        
        for line_index, line in enumerate(token_lines):
            for token_index, token in enumerate(line):
                # Check if the token is available controls
                if token in available_controls:
                    if token == "PLACE":
                        start_moving = True
                    else:
                        if not start_moving:
                            await self.warning_event.invoke("", f"(line {line_index + 1})WARNING! Command will not run without PLACE in the front")
                else: 
                    # It is PLACE line
                    if line[token_index - 1] == "PLACE":
                        # Check if there is essential information for PLACE
                        parts = token.split(",")
                        if len(parts) == 3 and parts[0].isdigit() and parts[1].isdigit() and parts[2] in Directions.__members__:
                            continue
                        else:
                            await self.warning_event.invoke("", f"ERROR! (line {line_index + 1})Invalie info for PLACE: {token}")                
                            return False 
                    # It is not PLACE line
                    else: 
                        await self.warning_event.invoke("", f"ERROR! (line {line_index + 1})Invalie token: {token}")                            
                        return False
        return True