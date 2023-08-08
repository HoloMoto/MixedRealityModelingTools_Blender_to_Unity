import bpy
import socket
import threading
import msgpack

class ClientHandler(threading.Thread):
    def __init__(self, client_socket):
        threading.Thread.__init__(self)
        self.client_socket = client_socket

    def run(self):
        while True:
            request = self.client_socket.recv(1024)
            data, address = sock.recvfrom(1073741824)
            if not request:
                break
            print("[*] Received: %s" % request.decode())
        self.client_socket.close()

class ServerThread(threading.Thread):
    def __init__(self, bind_ip, bind_port):
        threading.Thread.__init__(self)
        self.bind_ip = bind_ip
        self.bind_port = bind_port

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
            self.client_socket = client

# Run the server in a new thread
server_thread = ServerThread("0.0.0.0", 9998)
server_thread.start()

def send_message_to_unity(message):
    server_thread.client_socket.send(message.encode())

class SimpleOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.send_message"
    bl_label = "Send Message"

    def execute(self, context):
        ToUnityMessage()
        mesh_data = get_mesh_data()
        send_mesh_data(address, mesh_data)
        #print(mesh_data)
        return {'FINISHED'}

#BlenderAddonInfo
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


def ToUnityMessage():
    send_message_to_unity("Hello, Unity!")

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
    vertices = [tuple(v.co) for v in temp_mesh.vertices]

    triangles = []
    for p in temp_mesh.polygons:
        triangles.extend(p.vertices)

    # Remove Triangulate modifier
    obj.modifiers.remove(triangulate_mod)

  # Get normals
    normals = [tuple(v.normal) for v in temp_mesh.vertices]
    
    # Don't forget to remove the temporary mesh data
    bpy.data.meshes.remove(temp_mesh)
    print(f"Mesh data generated: vertices={len(vertices)}, triangles={len(triangles)}, normals={len(normals)}")
    return (vertices, triangles, normals)

def send_mesh_data(address, mesh_data):
    serialized_mesh_data = msgpack.packb(mesh_data, use_bin_type=True)
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.connect(address)
        sock.sendall(serialized_mesh_data)
        print(f"Mesh data sent to {address}")
