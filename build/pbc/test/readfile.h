#ifndef readfile_h
#define readfile_h

#include <stdio.h>
#include <stdlib.h>

static void
read_file (const char *filename , struct pbc_slice *slice) {
	FILE *f = fopen(filename, "rb");
	if (f == NULL) {
		slice->buffer = NULL;
		slice->len = 0;
		return;
	}
	fseek(f,0,SEEK_END);
	slice->len = ftell(f);
	fseek(f,0,SEEK_SET);
	slice->buffer = malloc(slice->len);
	if (fread(slice->buffer, 1 , slice->len , f) == 0)
		exit(1);
	fclose(f);
}

#endif
