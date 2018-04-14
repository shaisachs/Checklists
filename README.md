# Checklists

An API to keep track of recurring checklists. Read more about it on [my blog](https://shaisachs.github.io/2018/04/14/checklists-framework.html?src=github)!

## APIs

### Creating a template

```
POST /api/v1/checklistTemplates
{
  "name": "Open heart surgery"
}
```

### Creating items in a template

```
POST /api/v1/checklistTemplates/1/items
{
  "name": "Wash hands",
  "description": "Lather with soap, rinse for thirty seconds."
}
```

### Creating checklists based off of a template

```
POST /api/v1/checklists
{
	"name": "Surgery for the Tin Man",
	"checklistTemplateId": 1
}
```

Once a checklist is created, it will automatically be populated with checklist items that correspond to the items in the specified checklist template.

### Working with checklist items

Get the items in a checklist:

```
GET /api/v1/checklists/1/items

200 OK
{
	"items": [
		{
			"id": 1,
			"parentId": 1,
			"checklistTemplateItemId": 1,
			"completed": null
		}
	]
}
```

Mark a checklist item complete:

```
PUT /api/v1/checklists/1/items/1
{
	"id": 1,
	"parentId": 1,
	"checklistTemplateItemId": 1,
	"completed": "2018-01-01 11:00:00"
}
```
