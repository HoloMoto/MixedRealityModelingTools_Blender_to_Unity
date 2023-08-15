import bpy
import socket
import threading
import msgpack
import struct

class ClientHandler(threading.Thread):
    def __init__(self, client_socket):
        threading.Thread.__init__(self)
        self.client_socket = client_socket

    def run(self):
        try:
            while True:
                request = self.client_socket.recv(1024)
                if not request:
                    break
                print("[*] Received: %s" % request.decode())
        except Exception as e:
            print(f"Error while handling client: {e}")
        finally:
            self.client_socket.close()

class ServerThread(threading.Thread):
    def __init__(self, bind_ip, bind_port):
        threading.Thread.__init__(self)
        self.bind_ip = bind_ip
        self.bind_port = bind_port
        self.clients = []

    def run(self):
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.bind((self.bind_ip, self.bind_port))
        server.listen(5)
        print("[*] Listening on %s:%d" % (self.bind_ip, self.bind_port))

        while True:
            client, addr = server.accept()
            print("[*] Accepted connection from: %s:%d" % (addr[0], addr[1]))
            client_handler = ClientHandler(client)
            client_handler.start()
            self.clients.append(client)

    def close_all_clients(self):
        for client in self.clients:
            client.close()

# Run the server in a new thread
server_thread = ServerThread("0.0.0.0", 9998)
server_thread.start()

def send_message_to_unity(message):
    for client in server_thread.clients:
        try:
            client.send(message.encode())
        except Exception as e:
            print(f"Error while sending message to client: {e}")
            
import numpy as np

'''
def send_mesh_data_to_unity(mesh_data):
    vertices, triangles, normals = mesh_data

    # Convert vertices and normals to float32
    vertices_array = np.array(vertices, dtype='>f4').flatten() # float32 in big-endian
    normals_array = np.array(normals, dtype='>f4').flatten()   # float32 in big-endian
    triangles_array = np.array(triangles, dtype='>i4').flatten() # int32 in big-endian

    
    # Combine all arrays
    combined_array = np.concatenate((vertices_array, normals_array, triangles_array))

    # Convert numpy array to bytes
    serialized_mesh_data = combined_array.tobytes()

    for client in server_thread.clients:
        try:
            print(serialized_mesh_data)  # Print the binary data
            client.sendall(serialized_mesh_data)
        except Exception as e:
            print(f"Error while sending mesh data to client: {e}")
'''
def send_mesh_data_to_unity(mesh_data):
    vertices, triangles, normals = mesh_data

    # Convert numpy arrays to list
    vertices_list = np.array(vertices, dtype='<f4').flatten().tolist()
    triangles_list = np.array(triangles, dtype='<i4').flatten().tolist()
    normals_list = np.array(normals, dtype='<f4').flatten().tolist()

    # データを辞書として構築
    data_dict = {
        'vertices': vertices_list,
        'triangles': triangles_list,
        'normals': normals_list
    }

    # MessagePackでシリアライズ
    serialized_mesh_data = msgpack.packb(data_dict)

    print(f"Serialized data (bytes): {serialized_mesh_data.hex()}")

    # ここでデシリアライズの確認を行う
    try:
        deserialized_data = msgpack.unpackb(serialized_mesh_data)
        print("Deserialization success!")
        print(deserialized_data)  # もし必要ならば、デシリアライズされたデータを出力する
    except Exception as e:
        print(f"Deserialization error: {e}")
        return  # エラーが発生した場合、関数をここで終了する

    for client in server_thread.clients:
        try:
            client.sendall(serialized_mesh_data)
            print(serialized_mesh_data)
          
        except Exception as e:
            print(f"Error while sending mesh data to client: {e}")



class SimpleOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.send_message"
    bl_label = "Send Message"

    def execute(self, context):
       # send_message_to_unity("Hello from Blender!")
        mesh_data = get_mesh_data()
        send_mesh_data_to_unity(mesh_data)
        return {'FINISHED'}

class CustomPanel(bpy.types.Panel):
    """Creates a Panel in the Object properties window"""
    bl_label = "Send Message Panel"
    bl_idname = "OBJECT_PT_hello"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = "object"

    def draw(self, context):
        layout = self.layout
        layout.operator("object.send_message")

bpy.utils.register_class(SimpleOperator)
bpy.utils.register_class(CustomPanel)

def get_mesh_data():
    obj = bpy.context.view_layer.objects.active
    print(f"Active object name: {obj.name}")

    # Add Triangulate modifier
    triangulate_mod = obj.modifiers.new(name="Triangulate", type='TRIANGULATE')
    triangulate_mod.keep_custom_normals = True
    triangulate_mod.quad_method = 'BEAUTY'
    triangulate_mod.ngon_method = 'BEAUTY'

    # Apply modifiers and get the new mesh data
    bpy.context.view_layer.update()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    obj_eval = obj.evaluated_get(depsgraph)
    temp_mesh = bpy.data.meshes.new_from_object(obj_eval)

    # Get vertices and triangles
    vertices = [[v.co.x, v.co.y, v.co.z] for v in temp_mesh.vertices]  # この部分を変更

    triangles = []
    for p in temp_mesh.polygons:
        triangles.extend(p.vertices)

    # Remove Triangulate modifier
    obj.modifiers.remove(triangulate_mod)

    # Get normals
    normals = [[v.normal.x, v.normal.y, v.normal.z] for v in temp_mesh.vertices]  # この部分を変更

    # Don't forget to remove the temporary mesh data
    bpy.data.meshes.remove(temp_mesh)
    print(f"Mesh data generated: vertices={len(vertices)}, triangles={len(triangles)}, normals={len(normals)}")
    return (vertices, triangles, normals)


