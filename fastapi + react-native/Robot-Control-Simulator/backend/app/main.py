from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from api.simulator import Simulator
from pydantic import BaseModel
import uvicorn
from deps import Directions
import asyncio

app = FastAPI()

# Allow expo app to call this API
app.add_middleware(
    CORSMiddleware,
    allow_credentials=True,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

class ControlCode(BaseModel):
    text: str

@app.get("/")
def home():
    return "This is the backend of Robot-Control-Simulator."

# @app.post("/process") 
# async def process(code: ControlCode):
#     error_list, outputs_list = Simulator.simulate(code.text)
#     errors = '\n'.join(error_list)
#     outputs = '\n'.join(outputs_list)
#     return {"errors": errors, "outputs": outputs}

@app.websocket("/process")
async def process(ws: WebSocket):
    #region Init a simulator
    sim = Simulator()
    
    async def on_warning(*args):
        await ws.send_json({"type":"warning", "content":args[0]})
    sim.warning_event.add_handler("",on_warning)
        
    async def on_place(*args):
        await ws.send_json({"type":"PLACE", "content":args[0]})
    sim.updatePreview_event.add_handler("PLACE", on_place)
        
    async def on_move(*args):
        await ws.send_json({"type":"MOVE", "content":args[0]})
    sim.updatePreview_event.add_handler("MOVE", on_move)
        
    async def on_rotate(*args):
        await ws.send_json({"type":"ROTATE", "content":args[0]})
    sim.updatePreview_event.add_handler("ROTATE", on_rotate)
        
    async def on_report(*args):
        await ws.send_json({"type":"REPORT", "content":args[0]})
    sim.updatePreview_event.add_handler("REPORT", on_report)
    #endregion
    
    await ws.accept()
    
    try:
        while True:
            code = await ws.receive_text()
            if code != '' :
                await sim.simulate(code, False, 1)
    except WebSocketDisconnect:
        print("WebSocket disconnected")
    except Exception as e:
        print(f"Error: {e}")
        await ws.close()
    
if __name__ == "__main__":
    uvicorn.run(app, port=8080, host="0.0.0.0")