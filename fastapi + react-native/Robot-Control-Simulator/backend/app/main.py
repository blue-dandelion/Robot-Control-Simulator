from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from api.simulator import Simulator
from pydantic import BaseModel
import uvicorn
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
    # #region Init a simulator
    # sim = Simulator()
    # sim.warning_event.add_handler()
    # sim.updatePreview_event.add_handler()
    # #endregion
    
    # await ws.accept()
    
    # try:
    #     while True:
    #         message = await ws.receive_text()
    #         if message != {} :
    #             for i in range(3):
    #                 await ws.send_json({"errors": "eee", "outputs": "ooo"})
    #                 await asyncio.sleep(1)
    # except WebSocketDisconnect:
    #     print("WebSocket disconnected")
    # except Exception as e:
    #     print(f"Error: {e}")
    #     await ws.close()
        
    # # sim.simulate(code)
    # # try:
    # #     sim.simulate(code)
    # # except Exception as e:
    # #     print(e)
    
    await ws.accept()
    
    await asyncio.sleep(1)
    
    await ws.close

if __name__ == "__main__":
    uvicorn.run(app, port=8080, host="0.0.0.0")