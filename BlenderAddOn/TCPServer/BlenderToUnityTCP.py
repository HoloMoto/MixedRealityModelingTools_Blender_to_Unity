#version 0.1.0
import bpy
import socket
import threading
import msgpack
import struct
import base64
import math
from mathutils import Quaternion
import numpy as np

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
                process_received_data(request)
                print("[*] Received:", request)
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
            

def send_mesh_data_to_unity(mesh_data):
    vertices, triangles, normals, uvs  = mesh_data 
   
    # Convert numpy arrays to list
    vertices_list = np.array(vertices, dtype='<f4').flatten().tolist()
    triangles_list = np.array(triangles, dtype='<i4').flatten().tolist()
    normals_list = np.array(normals, dtype='<f4').flatten().tolist()
    uvs_list = np.array(uvs, dtype='<f4').flatten().tolist()

        # Debug: Print UV values
    print("UV Values:")
    for i in range(0, len(uvs_list), 2):
        uv_x = uvs_list[i]
        uv_y = uvs_list[i + 1]
        print(f"UV[{i//2}]: ({uv_x}, {uv_y})")
    
    MESH_HEADER = "MESH"  # メッシュデータのヘッダー
    # Build data as Dictionary
    data_dict = {
        'header': MESH_HEADER,
        'objectname' : bpy.context.view_layer.objects.active.name,
        'vertices': vertices_list,
        'triangles': triangles_list,
        'normals': normals_list,
        'uvs': uvs_list
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
        #print(deserialized_data)  
    except Exception as e:
        print(f"Deserialization error: {e}")
        return  

    for client in server_thread.clients:
        try:
            client.sendall(serialized_mesh_data)
            print(serialized_mesh_data)
          
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

    send_base_color_texture: bpy.props.BoolProperty(name="Send BaseColor Texture", default=True)


    def execute(self, context):
        mat_data =  get_material_data()

        send_material_data_to_unity(mat_data , self.send_base_color_texture)

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
        layout.prop(context.scene, "send_base_color_texture")

bpy.utils.register_class(SimpleOperator)
bpy.utils.register_class(CustomPanel)
bpy.utils.register_class(MaterialSenderOperator)
bpy.types.Scene.send_base_color_texture = bpy.props.BoolProperty(name="Send BaseColor Texture", default=False)



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

    uvs = []
    for p in temp_mesh.polygons:
            for loop_index in p.loop_indices:
                uv = temp_mesh.uv_layers.active.data[loop_index].uv
                uvs.extend([uv.x, uv.y])


    # Remove Triangulate modifier
    obj.modifiers.remove(triangulate_mod)

    # Get normals
    normals = [[v.normal.x, v.normal.y, v.normal.z] for v in temp_mesh.vertices]

    # Don't forget to remove the temporary mesh data
    bpy.data.meshes.remove(temp_mesh)
    print(f"Mesh data generated: vertices={len(vertices)}, triangles={len(triangles)}, normals={len(normals)}, uvs={len(uvs)}")
    return (vertices, triangles, normals ,uvs)

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

def send_material_data_to_unity(mat_data,send_base_color_texture):


    for selected_material_name in mat_data:
        selected_material = bpy.data.materials.get(selected_material_name)
        MAT_HEADER = "MATE"
        if selected_material:
            base_color_node = selected_material.node_tree.nodes.get("Principled BSDF")
            if base_color_node:
                base_color = base_color_node.inputs["Base Color"].default_value
                print(f"Material: {selected_material_name}")
                print(f"Base Color (RGBA): ({base_color[0]}, {base_color[1]}, {base_color[2]}, {base_color[3]})")

                if send_base_color_texture:
                    get_texture(selected_material)

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

def get_texture(material):
    if material.use_nodes:  # check used node
        nodes = material.node_tree.nodes# get node tree
        for node in nodes:#ノードの数実行
            if node.type == 'BSDF_PRINCIPLED':  # プリンシパルBSDFシェーダーノードを検索
                base_color_socket = node.inputs["Base Color"]
                if base_color_socket.is_linked:  # BaseColorにリンクがあるかチェック
                    texture_node = base_color_socket.links[0].from_node
                    if texture_node.type == 'TEX_IMAGE':  # テクスチャノードを検索
                        texture = texture_node.image
                        texture_path = bpy.path.abspath(texture_node.image.filepath)
                        send_texture_data(texture_path,texture.name)

def send_texture_data(texture_path,texture_name):
    IMAGE_HEADER = "IMGE"


    with open(texture_path, 'rb') as f:
        image_data = f.read()

    image_data_base64 = base64.b64encode(image_data).decode('utf-8')

    data_dict = {
        'header': IMAGE_HEADER,
        'imagename': texture_name,  
        'imagedata': image_data_base64
    }

    serialized_image_data = msgpack.packb(data_dict)

    for client in server_thread.clients:
        try:
            print(serialized_image_data)
            client.sendall(serialized_image_data)
        except Exception as e:
            print(f"Error while sending image data to client: {e}")

def update_camera_position(new_position,new_rotation):
    camera = bpy.data.objects.get("Camera")  # カメラオブジェクトの名前を指定
    if camera:
        print(new_rotation)
        camera.location = new_position
        camera_rotation_rad = [math.radians(angle) for angle in new_rotation]
        camera.rotation_euler = camera_rotation_rad
        bpy.context.view_layer.update()
        print("Camera position updated:", new_position)
    else:
        print("Camera not found in the scene.")

def process_received_data(request):
    try:
        data = msgpack.unpackb(request, raw=False)
        header = data[0]

        if header == 'UCAM':
            position = data[1]
            rotation = data[2]
            update_camera_position(position,rotation)
        else:
            print("Unknown header:", header)

    except Exception as e:
        print(f"Error while processing received data: {e}")