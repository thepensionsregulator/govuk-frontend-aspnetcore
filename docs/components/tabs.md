# Tabs

[GDS Tabs component](https://design-system.service.gov.uk/components/tabs/)

## Example

```razor
TODO
```

![Tabs](../images/tabs.png)


## API

### `<govuk-tabs>`

| Attribute | Type | Description |
| --- | --- | --- |
| `id` | `string` | The `id` attribute for the tabs component. |
| `id-prefix` | `string` | The prefix to use when generating the `id` attribute for each item. Required unless each item has an `id` specified. |
| `title` | `string` | The title for the tabs table of contents. The default is `Contents`. |

### `<govuk-tabs-item>`

The content is the HTML to use within the tab.\
Must be inside a `<govuk-tabs>` element.

| Attribute | Type | Description |
| --- | --- | --- |
| `id` | `string` | The `id` attribute for the item. |
| `label` | `string` | *Required* The text label for the item. |
| link-* | | Additional attributes to add to the tab. |
