import { Alert, ScrollView, useColorScheme, View, Animated, PanResponder, LayoutChangeEvent } from 'react-native';
import ThemedView from './components/ThemedView';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useRef, useState } from 'react';
import ThemedButton from './components/ThemedButton';
import { Colors } from './constants/colors';
import ThemedText from './components/ThemedText';
import DecoLine from './components/DecoLine';
import WorkspacePreview, { WorkspacePreviewHandles } from './components/WorkspacePreview';
import ThemedTextInput from './components/ThemedTextInput';
import * as DocumentPicker from 'expo-document-picker';
import * as FileSystem from 'expo-file-system';
import * as Sharing from 'expo-sharing';
import Compass from './assets/Compass.svg';

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

  const [running, setRunning] = useState(false);
  const [runningLine, setRunningLine] = useState(false);

  const [timespan, setTimespan] = useState('1');
  const [code, setCode] = useState('');
  const [errors, setErrors] = useState('');
  const [outputs, setOutputs] = useState('');
  const [console, setConsole] = useState('');

  const [previewContainerWidth, setPreviewContainerWidth] = useState(0);
  const [previewContainerHeight, setPreviewContainerHeight] = useState(0);
  const [previewWidth, setPreviewWidth] = useState(0);
  const [previewHieght, setPreviewHeight] = useState(0);
  const onPreviewContainerLayout = (e: LayoutChangeEvent) => {
    const { width, height } = e.nativeEvent.layout;
    setPreviewContainerWidth(width);
    setPreviewContainerHeight(height);
  }
  const onPreviewSizeChange = (w: number, h: number) => {
    setPreviewWidth(w);
    setPreviewHeight(h);
  }

  const horizontalEnabled = previewWidth > previewContainerWidth;
  const verticalEnabled = previewHieght > previewContainerHeight;

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
            setErrors(prev => prev + data.content + '\n');
          }
          else if (type == "PLACE") {
            workspacePreviewRef.current?.place(data.content.x, data.content.y, parseInt(data.content.f, 10));
          }
          else if (type == "MOVE") {
            workspacePreviewRef.current?.move(data.content.x, data.content.y);
          }
          else if (type == "ROTATE") {
            workspacePreviewRef.current?.rotate(parseInt(data.content, 10));
          }
          else if (type == "REPORT") {
            setOutputs(prev => prev + data.content + '\n');
          }
          else if (type == "message") {
            setConsole(prev => prev += data.content + '\n');
          }
          else if (type == "end") {
            setRunning(false);
            setRunningLine(false);
          }
        }
        catch (err) {
          if (err instanceof Error)
            Alert.alert('Error', err.message);
        }
      }

      ws_process.current.onerror = (e) => {
        Alert.alert('Websocket Error', e.type);
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

    setRunning(false);
    setRunningLine(false);
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
  const run = (runline: boolean) => {
    if (code.trim().length === 0) return;

    if (ws_process.current && connected_ws_process) {
      reset();

      ws_process.current.send(JSON.stringify({
        type: 'run',
        content: {
          timespan: parseFloat(timespan),
          code: code,
          runline: runline
        }
      }));

      setRunning(true);
      setRunningLine(runline);
    }
  }

  const nextLine = () => {
    if (ws_process.current && connected_ws_process) {
      ws_process.current.send(JSON.stringify({
        type: 'next'
      }));
    }
  }

  const stop = () => {
    if (ws_process.current && connected_ws_process) {
      ws_process.current.send(JSON.stringify({
        type: 'stop'
      }))

      setRunning(false);
      setRunningLine(false);
    }
  }

  const reset = () => {
    setErrors('');
    setOutputs('');
    setConsole('');
    workspacePreviewRef.current?.reset();
  }
  //#endregion

  //#region IDEs
  const importCode = async () => {
    // Pick a string file
    const res = await DocumentPicker.getDocumentAsync({
      type: 'text/plain'
    });

    if (res.canceled) {
      // user hit "Cancel"
      return;
    }

    // Get the picked file
    const { uri, name, size, mimeType } = res.assets[0];

    // Read it as UTF-8
    const content = await FileSystem.readAsStringAsync(uri, {
      encoding: FileSystem.EncodingType.UTF8,
    });

    setCode(content);
  }

  const exportCode = async () => {
    try {
      const fileName = 'Robot-Control-Code.txt';
      const filePath = FileSystem.documentDirectory + fileName;

      // Write the file
      await FileSystem.writeAsStringAsync(filePath, code, {
        encoding: FileSystem.EncodingType.UTF8
      });

      // Share / Export
      if (!(await Sharing.isAvailableAsync())) {
        Alert.alert('Error', 'Sharing not available on this platform');
        return;
      }
      await Sharing.shareAsync(filePath, {
        mimeType: 'text/plain',
        dialogTitle: 'Share your .txt file',
        UTI: 'public.plain-text'
      })
    } catch (err) {
      if (err instanceof Error) {
        Alert.alert('Error saving/sharing file', err.message);
      }
    }
  }

  const clearCode = async () => {
    setCode('');
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
          <ThemedButton text='Reload' style={{ marginLeft: 10 }} onPress={reload} disabled={running} />
        </View>

        {/* Simulator Control */}
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedText style={{ marginRight: 5, alignSelf: 'center' }}>Timespan:</ThemedText>
          <ThemedTextInput style={{ width: 50, height: 40, alignSelf: 'center' }} value={timespan} onChangeText={setTimespan} />
          <ThemedText style={{ marginRight: 10, marginLeft: 5, alignSelf: 'center' }}>s</ThemedText>

          <ThemedButton text='Run' onPress={() => run(false)} disabled={running} />
          <ThemedButton text={runningLine ? 'Next Line' : 'Run Line'} style={{ marginLeft: 5 }} onPress={running ? nextLine : () => { run(true) }} disabled={running && !runningLine} />

          <DecoLine direction='Vertical' style={{ marginHorizontal: 10 }} />

          <ThemedButton text='Stop' onPress={stop} disabled={!running} />
        </View>
        <DecoLine direction='Horizontal' />

        <ScrollView>
          {/* Preview */}
          <View
            style={{ position: 'relative', flex: 1, height: 400, margin: 10 }}
            onLayout={onPreviewContainerLayout}>
            <ScrollView
              style={{ overflow: 'hidden' }} horizontal={true}
              showsHorizontalScrollIndicator={false}
              scrollEnabled={horizontalEnabled}
              nestedScrollEnabled
              onContentSizeChange={onPreviewSizeChange}>
              <ScrollView
                style={{ overflow: 'hidden' }}
                showsVerticalScrollIndicator={false}
                scrollEnabled={verticalEnabled}
                nestedScrollEnabled
                onContentSizeChange={onPreviewSizeChange}>
                <WorkspacePreview ref={workspacePreviewRef} />
              </ScrollView>
            </ScrollView>

            <Compass width={100} height={100} color={theme.decoLine} style={{ position: 'absolute', bottom: 5, right: 5 }} />

            <View style={{ position: 'absolute', top: 5, right: 5, gap: 5 }}>
              <ThemedButton text='+' style={{ width: 40, height: 40 }} onPress={() => workspacePreviewRef.current?.resize('+')} />
              <ThemedButton text='-' style={{ width: 40, height: 40 }} onPress={() => workspacePreviewRef.current?.resize('-')} />
            </View>
          </View>
          <DecoLine direction='Horizontal' />

          <ThemedButton text='Reset' onPress={reset} disabled={running} />

          {/* IDE */}
          <View style={{ alignItems: 'flex-start', padding: 10 }}>
            <ThemedText title={true}>Code</ThemedText>
            <View style={{ flexDirection: 'row', marginVertical: 5, gap: 5 }}>
              <ThemedButton text='Import from file' onPress={importCode} />
              <ThemedButton text='Export file' onPress={exportCode} />
              <View style={{ flex: 1 }} />
              <ThemedButton text='Clear' onPress={clearCode} />
            </View>
            <ThemedTextInput style={{ width: '100%', minHeight: 100, maxHeight: 200, backgroundColor: theme.background_tl }} value={code} multiline={true} onChangeText={setCode} />
          </View>

          {/* Error */}
          <View style={{ alignItems: 'flex-start', width: '100%', padding: 10 }}>
            <ThemedText title={true}>Error</ThemedText>
            <ThemedTextInput style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }} editable={false} value={errors} multiline={true} onChangeText={setErrors} />
          </View>

          {/* Output */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', padding: 10 }}>
            <ThemedText title={true}>Output</ThemedText>
            <ThemedTextInput style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }} editable={false} value={outputs} multiline={true} onChangeText={setOutputs} />
          </View>

          {/* Console */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', padding: 10 }}>
            <ThemedText title={true}>Console</ThemedText>
            <ThemedTextInput style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }} editable={false} value={console} multiline={true} onChangeText={setConsole} />
          </View>

          <View style={{ height: 500 }} />
        </ScrollView>

      </ThemedView >

    </SafeAreaProvider >

  );
}

