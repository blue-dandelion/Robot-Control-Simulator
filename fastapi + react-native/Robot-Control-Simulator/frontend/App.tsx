import { Styles } from './constants/styles';
import { Alert, Button, Pressable, ScrollView, Text, TextInput, useColorScheme, View } from 'react-native';
import ThemedView from './components/ThemedView';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useEffect, useRef, useState } from 'react';
import ThemedButton from './components/ThemedButton';
import { Colors } from './constants/colors';
import ThemedText from './components/ThemedText';
import DecoLine from './components/DecoLine';
import WorkspacePreview, { WorkspacePreviewHandles } from './components/WorkspacePreview';
import { Direction, RotateTo } from './constants/deps';
import ThemedTextInput from './components/ThemedTextInput';

export default function App() {
  const colorScheme = useColorScheme()
  const theme = colorScheme ? Colors[colorScheme] : Colors.light

  const [serverURL, setServerURL] = useState('');
  const ws_reload = useRef<WebSocket | null>(null);
  const ws_process = useRef<WebSocket | null>(null);
  const [connected_ws_reload, setConnected_ws_reload] = useState(false);
  const [connected_ws_process, setConnected_ws_process] = useState(false);

  const [workspaceWidth, setWorkspaceWidth] = useState('5')
  const [workspaceHeight, setWorkspaceHeight] = useState('5')
  const workspacePreviewRef = useRef<WorkspacePreviewHandles>(null)

  const [timespan, setTimespan] = useState('1');
  const [code, setCode] = useState('');
  const [errors, setErrors] = useState('');
  const [outputs, setOutputs] = useState('');
  const [console, setConsole] = useState('');

  //#region Server Connection
  const connect = () => {
    setConsole(prev => prev + `Connecting to server ${serverURL}...\n`)

    //#region /reload
    if (ws_reload.current == null) {
      const URL = `ws://${serverURL}/reload`
      ws_reload.current = new WebSocket(URL);

      ws_reload.current.onopen = () => {
        setConnected_ws_reload(true);
      }

      ws_reload.current.onmessage = (e) => {
        try {
          // Read the received message
          const data = JSON.parse(e.data);
          let type = data.type;

          if (type == "reload") {
            workspacePreviewRef.current?.reload(data.content.w, data.content.h)
          }
        }
        catch (err) {
          if (err instanceof Error)
            Alert.alert('Error', err.message)
        }
      }

      ws_reload.current.onerror = (e) => {
        Alert.alert('Websocket Error', e.type)
      }

      ws_reload.current.onclose = () => {
        setConnected_ws_reload(false);
        ws_reload.current = null;
      }
    }
    //#endregion

    //#region /process
    if (ws_process.current == null) {
      const URL = `ws://${serverURL}/process`
      ws_process.current = new WebSocket(URL);

      ws_process.current.onopen = () => {
        setConnected_ws_process(true);
      }

      ws_process.current.onmessage = (e) => {
        try {
          // Read the received message
          const data = JSON.parse(e.data);
          let type = data.type;

          if (type == "warning") {
            setErrors(prev => prev + data.content + '\n')
          }
          else if (type == "PLACE") {
            workspacePreviewRef.current?.place(data.content.x, data.content.y, parseInt(data.content.f, 10))
          }
          else if (type == "MOVE") {
            workspacePreviewRef.current?.move(data.content.x, data.content.y)
          }
          else if (type == "ROTATE") {
            workspacePreviewRef.current?.rotate(parseInt(data.content, 10))
          }
          else if (type == "REPORT") {
            setOutputs(prev => prev + data.content + '\n')
          }
          else if (type == "message") {
            setConsole(prev => prev += data.content + '\n')
          }
        }
        catch (err) {
          if (err instanceof Error)
            Alert.alert('Error', err.message)
        }
      }

      ws_process.current.onerror = (e) => {
        Alert.alert('Websocket Error', e.type)
      }

      ws_process.current.onclose = () => {
        setConnected_ws_process(false);
        ws_process.current = null;
      }
    }
    //#endregion
  }

  const disconnect = () => {
    if (ws_reload.current) {
      ws_reload.current.close();
    }
    if (ws_process.current) {
      ws_process.current.close();
    }
  }
  //#endregion

  //#region WorkSpace
  const reload = () => {
    if (ws_reload.current && connected_ws_reload) {
      ws_reload.current.send(JSON.stringify({
        w: parseInt(workspaceWidth, 10),
        h: parseInt(workspaceHeight, 10)
      }))
    }
  }
  //#endregion

  //#region Simulator Control
  const run = () => {
    if (ws_process.current && connected_ws_process) {
      ws_process.current.send(JSON.stringify({
        type: 'run',
        content: {
          timespan: parseFloat(timespan),
          code: code
        }
      }));
    }
  }

  const stop = () => {
    if (ws_process.current && connected_ws_process) {
      ws_process.current.send(JSON.stringify({
        type: 'stop'
      }))
    }
  }
  //#endregion

  return (
    <SafeAreaProvider>
      <ThemedView safe={true} style={{ flex: 1, flexDirection: 'column' }}>

        {/* Connection Control*/}
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedTextInput style={{ width: 200, height: 40, marginHorizontal: 10, alignSelf: 'center' }} placeholder='Server IP:Port' value={serverURL} onChangeText={setServerURL} />
          <ThemedButton text={connected_ws_reload && connected_ws_process ? "Disconnect" : "Connect"} onPress={connected_ws_reload && connected_ws_process ? disconnect : connect} />
        </View>

        {/* Workspace Settings */}
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedText title={true} style={{ alignSelf: 'center' }}>Workspace</ThemedText>
          <ThemedText style={{ alignSelf: 'center', marginLeft: 10, marginRight: 5 }}>w:</ThemedText>
          <ThemedTextInput style={{ width: 50, height: 40, alignSelf: 'center' }} value={workspaceWidth} onChangeText={setWorkspaceWidth} />
          <ThemedText style={{ alignSelf: 'center', marginLeft: 10, marginRight: 5 }}>h:</ThemedText>
          <ThemedTextInput style={{ width: 50, height: 40, alignSelf: 'center' }} value={workspaceHeight} onChangeText={setWorkspaceHeight} />
          <ThemedButton text='Reload' style={{ marginLeft: 10 }} onPress={reload} />
        </View>

        {/* Simulator Control */}
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedText style={{ marginRight: 5, alignSelf: 'center' }}>Timespan:</ThemedText>
          <ThemedTextInput style={{ width: 50, height: 40, alignSelf: 'center' }} value={timespan} onChangeText={setTimespan} />
          <ThemedText style={{ marginRight: 10, marginLeft: 5, alignSelf: 'center' }}>s</ThemedText>

          <ThemedButton text='Run' onPress={run} />
          <ThemedButton text='Run Line' style={{ marginLeft: 5 }} />

          <DecoLine direction='Vertical' style={{ marginHorizontal: 10 }} />

          <ThemedButton text='Stop' onPress={stop} />
        </View>
        <DecoLine direction='Horizontal' />

        <ScrollView>
          {/* Preview */}
          <View style={{ backgroundColor: '#000', height: 400, alignItems: 'center', justifyContent: 'center' }}>
            <WorkspacePreview ref={workspacePreviewRef} />
          </View>

          {/* IDE */}
          <View style={{ alignItems: 'flex-start', height: 'auto', padding: 10 }}>
            <ThemedText title={true}>Code</ThemedText>
            <ThemedTextInput style={{ width: '100%', minHeight: 100, maxHeight: 200, backgroundColor: theme.background_tl }} value={code} multiline={true} onChangeText={setCode} />
          </View>

          {/* Error */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Error</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={errors} multiline={true} onChangeText={setErrors} />
            </ScrollView>
          </View>

          {/* Output */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Output</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={outputs} multiline={true} onChangeText={setOutputs} />
            </ScrollView>
          </View>

          {/* Console */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Console</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={console} multiline={true} onChangeText={setConsole} />
            </ScrollView>
          </View>

          <View style={{ height: 500 }} />
        </ScrollView>

      </ThemedView >

    </SafeAreaProvider >

  );
}

