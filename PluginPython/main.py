import sys
import gtts
import os
import shutil

if __name__ == '__main__':

    msg = sys.argv[1]
    len = sys.argv[2]
    path = sys.argv[3]

    if len == "es":
        accent = 'com.mx'
    elif len == "fr":
        accent = "fr"
    else:
        accent = "com.au"
        len = "en"

    file = 'output.mp3'

    output = gtts.tts.gTTS(text=msg,lang=len,slow=False, tld=accent)
    output.save(file)

    #os.system(f"start {file}")

    shutil.move(file, f"{path}\\{file}")
