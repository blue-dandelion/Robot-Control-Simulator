import os

def import_file(file_path: str) -> str:
    # Make sure file exist
    if not os.path.exists(file_path):
        return None
    # If file exists, read the file to string
    with open(file_path, "r") as file:
        return file.read()   