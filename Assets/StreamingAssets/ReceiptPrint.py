import os
import sys
import json
from escpos.printer import Usb
from datetime import datetime

if getattr(sys, 'frozen', False):
    application_path = sys._MEIPASS
    application_path = application_path.replace("\\","/")
else:
    application_path = os.path.dirname(os.path.abspath(__file__))

os.chdir(application_path)

currentTime = datetime.now()
formattedTime = currentTime.strftime("%m-%d-%Y %I:%M%p")

jsonPath = "ReceiptResources/receiptContents.json"
contents = open(jsonPath,'r')
jsonData = json.loads(contents.read())
contents.close()

title = jsonData["title"]
body = jsonData["body"]
playerNum = jsonData["playerNum"]
tags = jsonData["tags"]
hasQR = jsonData["hasQR"]

printer = Usb(0x0416,0x5011,profile="NT-5890K")

printer.set(align="center",double_width=True);
printer.text("----------------")
printer.set(normal_textsize=True);

printer.set(align="center",underline=2,double_height=True);
printer.text("\n\n"+title+"\n")
printer.set(underline=0,normal_textsize=True);
printer.text(playerNum+"\n")
printer.set(underline=1);
printer.text("\n"+tags+"\n")

printer.set(align="left",underline=0);
bodyLines = body.splitlines()
for line in bodyLines:
    printer.text("\n")
    printer.block_text(line, columns=32)

if (hasQR):
    qrLink = jsonData["qrLink"]
    printer.text("\n")
    printer.qr(qrLink, ec=0, size=6, model=2, native=False, center=True, impl=None, image_arguments=None)

printer.text("\n\n"+formattedTime)
printer.set(align="right")
printer.set(double_width=True)
printer.text("\n----------------")
printer.set(align="center",double_width=True)
printer.text("\n\nTHANK YOU\nCOME AGAIN\n")
printer.text("\n----------------")
printer.text("\n\n\n\n")
