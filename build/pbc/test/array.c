#include "pbc.h"
#include "alloc.h"
#include "array.h"

#include <stdio.h>

int
main()
{
	pbc_array array;
	pbc_var v;

	_pbcA_open(array);

	int i ;

	for (i=0;i<100;i++) {
		v->real = (double)i;
		printf("push %d\n",i);
		_pbcA_push(array, v);
	}

	int s = pbc_array_size(array);

	for (i=0;i<s;i++) {
		_pbcA_index(array, i , v);
		printf("%lf\n",v->real);
	}

	_pbcA_close(array);

	return 0;
}
