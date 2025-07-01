from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from api.simulator import Simulator
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

@app.get("/")
def home():
    return "This is the backend of Robot-Control-Simulator."

# @app.post("/process") 
# async def process(code: ControlCode):
#     error_list, outputs_list = Simulator.simulate(code.text)
#     errors = '\n'.join(error_list)
#     outputs = '\n'.join(outputs_list)
#     return {"errors": errors, "outputs": outputs}

sim = Simulator()

@app.websocket("/reload")
async def reload(ws_reload: WebSocket):
    await ws_reload.accept()

    #region Add events to simulator

    #endregion

    try:
        while True:
            data = await ws_reload.receive_json()

            if data is not None:
                sim.workspaceWidth = data['w']
                sim.workspaceHeight = data['h']
                await ws_reload.send_json({"type": "reload", "content": {"w": sim.workspaceWidth, "h": sim.workspaceHeight}})
    except WebSocketDisconnect:
        print(".../reload WebSocket disconnected")
    except Exception as e:
        print(f".../reload Error: {e}")
        await ws_reload.close()

@app.websocket("/process")
async def process(ws_process: WebSocket):    
    await ws_process.accept()

    #region Add events to simulator
    async def on_warning(*args):
        await ws_process.send_json({"type":"warning", "content":args[0]})
    sim.warning_event.add_handler("",on_warning)
        
    async def on_place(*args):
        await ws_process.send_json({"type":"PLACE", "content":args[0]})
    sim.updatePreview_event.add_handler("PLACE", on_place)
        
    async def on_move(*args):
        await ws_process.send_json({"type":"MOVE", "content":args[0]})
    sim.updatePreview_event.add_handler("MOVE", on_move)
        
    async def on_rotate(*args):
        await ws_process.send_json({"type":"ROTATE", "content":args[0]})
    sim.updatePreview_event.add_handler("ROTATE", on_rotate)
        
    async def on_report(*args):
        await ws_process.send_json({"type":"REPORT", "content":args[0]})
    sim.updatePreview_event.add_handler("REPORT", on_report)

    async def on_end():
        await ws_process.send_json({"type": "end"})
    sim.end_event.add_handler("", on_end)
    #endregion
    
    sim_task = None
    nextline_event = None
    try:
        while True:
            data = await ws_process.receive_json()
            type = data['type']

            if(type == 'run'):
                # if there's already a running task, cancel it
                if sim_task and not sim_task.done():
                    sim_task.cancel()
                #launch a new task
                nextline_event = asyncio.Event()
                sim_task = asyncio.create_task(sim.simulate(nextline_event, data['content']['code'], data['content']['runline'], data['content']['timespan']))
            elif(type == 'next'):
                nextline_event.set()
            elif(type == 'stop'):
                if sim_task:
                    sim_task.cancel()
                    await ws_process.send_json({"type":"message", "content":"Code stops running"})
                    await on_end()
                    sim_task = None
                    nextline_event = None
    except WebSocketDisconnect:
        print(".../process WebSocket disconnected")
        if sim_task:
            sim_task.cancel()
    except Exception as e:
        print(f".../process Error: {e}")
        sim_task.cancel()
        await ws_process.close()
    finally:
        #region Remove events to simulator
        sim.warning_event.remove_handler("",on_warning)
        sim.updatePreview_event.remove_handler("PLACE", on_place)
        sim.updatePreview_event.remove_handler("MOVE", on_move)
        sim.updatePreview_event.remove_handler("ROTATE", on_rotate)
        sim.updatePreview_event.remove_handler("REPORT", on_report)
        #endregion

    
if __name__ == "__main__":
    uvicorn.run(app, port=8000, host="0.0.0.0")