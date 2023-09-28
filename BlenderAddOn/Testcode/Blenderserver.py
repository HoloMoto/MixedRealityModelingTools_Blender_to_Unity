import bpy
import msgpack
import socket
import threading
import time


stop_server = False

def udp_server(sock, server_address):
    sock.bind(server_address)
    print(f"Server is running on {server_address}")

    while True:
        data, address = sock.recvfrom(1073741824)
        print(f"Received message from {address}: {data.decode('utf-8')}")
 
        if data.decode('utf-8') == "GET_MESH":
            mesh_data = get_mesh_data()
            send_mesh_data(address, mesh_data)

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

  # 頂点の法線を取得
    normals = [tuple(v.normal) for v in temp_mesh.vertices]
    
    # Don't forget to remove the temporary mesh data
    bpy.data.meshes.remove(temp_mesh)
    print(f"Mesh data generated: vertices={len(vertices)}, triangles={len(triangles)}, normals={len(normals)}")
    return (vertices, triangles, normals)

def send_mesh_data(address, mesh_data):
    serialized_mesh_data = msgpack.packb(mesh_data, use_bin_type=True)
    sock.sendto(serialized_mesh_data, address)
    print(f"Mesh data sent to {address}")
    time.sleep(0.5)


sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server_address = ("0.0.0.0",12347)
#server_address = ("127.0.0.1", 12347)

t = threading.Thread(target=udp_server, args=(sock, server_address))
t.daemon = True
t.start()
