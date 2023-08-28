#version 0.0.12
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


def send_mesh_data_to_unity(mesh_data):
    vertices, triangles, normals = mesh_data

    # Convert numpy arrays to list
    vertices_list = np.array(vertices, dtype='<f4').flatten().tolist()
    triangles_list = np.array(triangles, dtype='<i4').flatten().tolist()
    normals_list = np.array(normals, dtype='<f4').flatten().tolist()
    MESH_HEADER = "MESH"  # メッシュデータのヘッダー
    # Build data as Dictionary
    data_dict = {
        'header': MESH_HEADER,
        'objectname' : bpy.context.view_layer.objects.active.name,
        'vertices': vertices_list,
        'triangles': triangles_list,
        'normals': normals_list
    }

    # serialize MessagePack
    serialized_mesh_data = msgpack.packb(data_dict)

    #print(f"Serialized data (bytes): {serialized_mesh_data.hex()}")
    #Verification
    #verification_mesh_data(serialized_mesh_data)
      
    # Check Deserialization
    try:
        deserialized_data = msgpack.unpackb(serialized_mesh_data)
        print("Deserialization success!")
        #print(deserialized_data)  # もし必要ならば、デシリアライズされたデータを出力する
    except Exception as e:
        print(f"Deserialization error: {e}")
        return  # エラーが発生した場合、関数をここで終了する

    for client in server_thread.clients:
        try:
            client.sendall(serialized_mesh_data)
            #print(serialized_mesh_data)
          
        except Exception as e:
            print(f"Error while sending mesh data to client: {e}")



class SimpleOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.send_message"
    bl_label = "Send Mesh"

    def execute(self, context):
       # send_message_to_unity("Hello from Blender!")
        mesh_data = get_mesh_data()
        send_mesh_data_to_unity(mesh_data)
        return {'FINISHED'}

class MaterialSenderOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.send_mat"
    bl_label = "Send Mat"

    def execute(self, context):
        mat_data =  get_material_data()
        send_material_data_to_unity(mat_data)
        return{'FINISHED'}

class CustomPanel(bpy.types.Panel):
    """Creates a Panel in the Object properties window"""
    bl_label = "MixedRealityModelingTools for"
    bl_idname = "OBJECT_PT_hello"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = "object"

    def draw(self, context):
        layout = self.layout
        layout.operator("object.send_message")
        layout.operator("object.send_mat")

bpy.utils.register_class(SimpleOperator)
bpy.utils.register_class(CustomPanel)
bpy.utils.register_class(MaterialSenderOperator)

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
    bpy.ops.mesh.customdata_custom_splitnormals_clear()
    
    # Get vertices and triangles
    vertices = [[v.co.x, v.co.y, v.co.z] for v in temp_mesh.vertices] 

    triangles = []
    for p in temp_mesh.polygons:
        triangles.extend(p.vertices)

    # Remove Triangulate modifier
    obj.modifiers.remove(triangulate_mod)

    # Get normals
    normals = [[v.normal.x, v.normal.y, v.normal.z] for v in temp_mesh.vertices]

    # Don't forget to remove the temporary mesh data
    bpy.data.meshes.remove(temp_mesh)
    print(f"Mesh data generated: vertices={len(vertices)}, triangles={len(triangles)}, normals={len(normals)}")
    return (vertices, triangles, normals)

def verification_mesh_data(serialized_mesh_data):

    data_dict = msgpack.unpackb(serialized_mesh_data)
    
    # 2. データから頂点、三角形、法線のリストを取得
    vertices = [(data_dict['vertices'][i], data_dict['vertices'][i+1], data_dict['vertices'][i+2]) for i in range(0, len(data_dict['vertices']), 3)]

    triangles = [(data_dict['triangles'][i], data_dict['triangles'][i+1], data_dict['triangles'][i+2]) for i in range(0, len(data_dict['triangles']), 3)] 

    normals = [(data_dict['normals'][i], data_dict['normals'][i+1], data_dict['normals'][i+2]) for i in range(0, len(data_dict['normals']), 3)]

    
    # 3. 新しいメッシュを作成
    mesh = bpy.data.meshes.new(name="VerifiedMesh")
    mesh.from_pydata(vertices, [], triangles)
    
    # 法線を設定 (オプション: 必要に応じてコメントアウトしても良い)
    mesh.normals_split_custom_set_from_vertices(normals)
    mesh.use_auto_smooth = True
    
    # 4. メッシュをシーンに追加
    obj = bpy.data.objects.new("VerifiedMeshObject", mesh)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    
    return obj

def get_material_data():
    selected_objects = bpy.context.selected_objects

    material_names = []

    for obj in selected_objects:
      if obj.type == 'MESH':  # メッシュオブジェクトのみ処理する
        for slot in obj.material_slots:
            if slot.material:
                material_names.append(slot.material.name)

    return material_names

def send_material_data_to_unity(mat_data):
    for selected_material_name in mat_data:
        selected_material = bpy.data.materials.get(selected_material_name)
        MAT_HEADER = "MATE"
        if selected_material:
            base_color_node = selected_material.node_tree.nodes.get("Principled BSDF")
            if base_color_node:
                base_color = base_color_node.inputs["Base Color"].default_value
                print(f"Material: {selected_material_name}")
                print(f"Base Color (RGBA): ({base_color[0]}, {base_color[1]}, {base_color[2]}, {base_color[3]})")

                rgba = list(base_color)
                rgba_list = np.array(rgba, dtype='<f4').flatten().tolist()

                mat_data_dict = {
                    'header': MAT_HEADER,
                    'materialname': [selected_material_name],
                    'rgba': rgba_list
                }

                serialized_mat_data = msgpack.packb(mat_data_dict)
                print(serialized_mat_data)
                print("material_Send")
                for client in server_thread.clients:
                    try:
                        client.sendall(serialized_mat_data)
                    except Exception as e:
                        print(f"Error while sending mesh data to client: {e}")
        else:
            print("Invalid material name")
            print(f"Error while sending mesh data to client: {e}")