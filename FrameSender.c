#include <assert.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "osdep/osdep.h"

static struct wif* _wi_out;

const int maxFrameSize = 2306;
const int headerSize = 16;
const int maxPayloadSize = 2290; // Including space from SA and Seq Number - i don't need all these system reserved fields and will use them to increase payload for my transmitter.

void TransmitFrame(unsigned char* payload, int payloadSize)
{
	assert(payloadSize <= maxPayloadSize);

	int frameSize = headerSize + payloadSize;
	assert(frameSize <= maxFrameSize);

	unsigned char* frame = malloc(frameSize);
	char header[] = // TODO: Load this from config file
	   {
	       0x08,0x42, // Frame Type
	       0x00,0x00, // Duration
	       0x00,0x00,0x00,0x00,0x00,0x00, // STA Addr
	       0xf0,0x27,0x65,0x7d,0xff,0xf2 // BSSID
	   };

	memcpy(frame, header, headerSize);
	memcpy(&frame[headerSize], payload, payloadSize);

	int writeResult = wi_write(_wi_out, frame, frameSize, NULL);
	free(frame);
	if (writeResult == -1)
		perror("Frame transmitting error.");
}